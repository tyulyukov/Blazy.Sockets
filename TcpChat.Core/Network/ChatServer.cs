using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;
using TcpChat.Core.Interfaces;

namespace TcpChat.Core.Network;

public class ChatServer : IDisposable
{
    private readonly IHandlersCollection _handlers;
    private readonly CancellationToken _cancellationToken;
    private readonly Socket _listener;
    private readonly IPEndPoint _ipEndPoint;
    private readonly List<Socket> _clients;
    private readonly object _locker;
    private readonly ILogHandler _logger;

    public ChatServer(IPEndPoint ipEndPoint, IHandlersCollection handlers, CancellationToken cancellationToken, ILogHandler logger)
    {
        _clients = new();
        _locker = new();
        _ipEndPoint = ipEndPoint;
        _cancellationToken = cancellationToken;
        _logger = logger;
        _handlers = handlers;
        _listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task Run()
    {
        try
        {
            _listener.Bind(_ipEndPoint);
            _listener.Listen(100);

            _logger.HandleText("Server is running");

            while (!_cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptAsync(_cancellationToken);
                AcceptClient(client);
            }
        }
        catch (Exception ex) when(ex is not OperationCanceledException)
        {
           _logger.HandleError(ex);
        }
        finally
        {
            _listener.Close();
            _logger.HandleText("Server stopped");
        }
    }
    
    private void AcceptClient(Socket client)
    {
        _logger.HandleText($"Connection from {client.RemoteEndPoint}");
        
        lock (_locker)
            _clients.Add(client);
        
        Task.Run(() => ReceivePacketsAsync(client), _cancellationToken);
    }

    private async void ReceivePacketsAsync(Socket client)
    {   
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var stream = new NetworkStream(client);
                var request = await JsonSerializer.DeserializeAsync<Packet>(stream, cancellationToken: _cancellationToken);

                if (request is null)
                    continue;

                _logger.HandleText($"Received from {client.RemoteEndPoint}");

                var handler = _handlers.Resolve(request.Event);

                if (handler is null)
                {
                    var packet = new Packet
                    {
                        Event = "Error",
                        State = new { Message = $"Handler was not found for <{request.Event}> event." }
                    };
                    
                    await JsonSerializer.SerializeAsync(stream, packet, cancellationToken: _cancellationToken);
                    continue;
                }

                await handler.HandleAsync(request, client, _cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.HandleText($"Disconnected {client.RemoteEndPoint}");
        }
        finally
        {
            lock (_locker)
                _clients.Remove(client);
        }
    }

    public void Dispose()
    {
        lock (_clients)
            for (int i = 0; i < _clients.Count; i++)
                _clients[i].Close();
        
        _listener.Close();
    }
}