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
                AcceptClient(client, ct);
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
    
    private void AcceptClient(Socket client, CancellationToken ct)
    {
        _logger.HandleText($"Connection from {client.RemoteEndPoint}");
        
        lock (_threadLocker)
            _clients.Add(client);
        
        Task.Run(() => ReceivePacketsAsync(client, ct), ct);
    }

    private async void ReceivePacketsAsync(Socket client, CancellationToken ct)
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

                _logger.HandleText($"Incoming packet from {client.RemoteEndPoint}");

                var handler = _handlers.Resolve(request.Event);

                if (handler is null)
                {
                    var message = $"Handler was not found for {request.Event} event that sent by {client.RemoteEndPoint}.";
                    _logger.HandleText(message);
                    
                    var packet = new Packet
                    {
                        Event = "Error",
                        State = new { Message = $"Handler was not found for {request.Event} event." }
                    };

                    var response = _packetEncoder.Encode(packet);
                    _ = await client.SendAsync(response, SocketFlags.None, ct);
                    continue;
                }

                _logger.HandleText($"Packet from {client.RemoteEndPoint} handled by {handler.GetType()}");
                
                handler.BeginSocketScope(client);
                await handler.HandleAsync(request, ct);
                handler.EndSocketScope();
            }
        }
        catch (Exception exception)
        {
            _logger.HandleText($"Disconnected {client.RemoteEndPoint}");
        }
        finally
        {
            lock (_threadLocker)
                _clients.Remove(client);
        }
    }

    public void Dispose()
    {
        lock (_clients)
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