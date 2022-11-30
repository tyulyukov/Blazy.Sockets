using System.Net;
using System.Net.Sockets;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Core.Network;

public class ChatServer : INetworkServer
{
    private readonly IHandlersCollection _handlers;
    private readonly Socket _listener;
    private readonly IPEndPoint _ipEndPoint;
    private readonly List<Socket> _clients;
    private readonly object _threadLocker;
    private readonly ILogHandler _logger;
    private readonly IEncoder<Packet> _packetEncoder;

    public ChatServer(IPEndPoint ipEndPoint, IHandlersCollection handlers, ILogHandler logger, IEncoder<Packet> packetEncoder)
    {
        _clients = new();
        _threadLocker = new();
        _ipEndPoint = ipEndPoint;
        _logger = logger;
        _packetEncoder = packetEncoder;
        _handlers = handlers;
        _listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        try
        {
            _listener.Bind(_ipEndPoint);
            _listener.Listen(100);

            _logger.HandleText("Server is running");

            while (!ct.IsCancellationRequested)
            {
                var client = await _listener.AcceptAsync(ct);
                await AcceptClientAsync(client, DateTime.Now, ct);
            }
        }
        catch (Exception ex) when(ex is not OperationCanceledException && ex is not TaskCanceledException)
        {
           _logger.HandleError(ex);
        }
        finally
        {
            _logger.HandleText("Server stopped");
        }
    }
    
    private async Task AcceptClientAsync(Socket client, DateTime connectedAt, CancellationToken ct)
    {
        var connectionDetails = new ConnectionDetails()
        {
            ConnectedAt = connectedAt
        };
        
        lock (_threadLocker)
            _clients.Add(client);
        
        var handler = _handlers.ResolveConnectionHandler();

        if (handler is not null)
        {
            await handler.HandleWithScopedSocketAsync(client, connectionDetails, ct);
        }
        
        _ = Task.Run(() => ReceivePacketsAsync(client, connectionDetails, ct), ct);
    }

    private async void ReceivePacketsAsync(Socket client, ConnectionDetails connectionDetails, CancellationToken ct)
    {   
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var buffer = new byte[8192];
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
        
        _listener.Close();
    }
}