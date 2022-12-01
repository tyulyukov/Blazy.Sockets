using System.Net.Sockets;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Core.Network;

public class SocketAcceptor : ISocketAcceptor
{
    private readonly IHandlersCollection _handlers;
    private readonly List<Socket> _clients;
    private readonly object _threadLocker;
    private readonly IEncoder<Packet> _packetEncoder;
    private readonly ILogHandler _logger;

    public SocketAcceptor(IHandlersCollection handlers, IEncoder<Packet> packetEncoder, ILogHandler logger)
    {
        _handlers = handlers;
        _clients = new ();
        _packetEncoder = packetEncoder;
        _logger = logger;
        _threadLocker = new ();
    }

    public async Task AcceptSocketAsync(Socket socket, CancellationToken ct)
    {
        var connectionDetails = new ConnectionDetails()
        {
            ConnectedAt = DateTime.Now
        };
        
        lock (_threadLocker)
            _clients.Add(socket);
        
        var handler = _handlers.ResolveConnectionHandler();

        if (handler is not null)
        {
            await handler.HandleWithScopedSocketAsync(socket, connectionDetails, ct);
        }
        
        _ = Task.Run(() => ReceivePacketsAsync(socket, connectionDetails, ct), ct);
    }
    
    private async void ReceivePacketsAsync(Socket client, ConnectionDetails connectionDetails, CancellationToken ct)
    {   
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var buffer = new byte[1024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None, ct);
                var request = _packetEncoder.Decode(buffer, received);

                if (request is null)
                    continue;

                // TODO middlewares here
                // _logger.HandleText($"Incoming packet from {client.RemoteEndPoint}");

                var handler = _handlers.Resolve(request.Event);

                if (handler is null)
                {
                    var message = $"Handler was not found for {request.Event} event";
                    await SendErrorAsync(client, message, ct);
                    continue;
                }
                
                // and here
                // _logger.HandleText($"Packet from {client.RemoteEndPoint} handled by {handler.GetType()}");

                await handler.ExecuteAsync(request.State, client, ct);
                
                // and maybe here
            }
        }
        catch (Exception exception)
        {
            var handler = _handlers.ResolveDisconnectionHandler();

            if (handler is not null)
            {
                await handler.HandleWithScopedSocketAsync(client, new DisconnectionDetails
                {
                    DisconnectedAt = DateTime.Now,
                    ConnectionTime = DateTime.Now - connectionDetails.ConnectedAt
                }, ct);
            }
        }
        finally
        {
            client.Close();
            
            lock (_threadLocker)
                _clients.Remove(client);
        }
    }

    private async Task SendErrorAsync(Socket client, string message, CancellationToken ct)
    {
        _logger.HandleText(message);
                    
        var packet = new Packet
        {
            Event = "Error",
            State = new { Message = message }
        };

        var response = _packetEncoder.Encode(packet);
        _ = await client.SendAsync(response, SocketFlags.None, ct);
    }
    
    public void Dispose()
    {
        lock (_threadLocker)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                _clients[i].Shutdown(SocketShutdown.Both);
                _clients[i].Close();
            }
        }
    }
}