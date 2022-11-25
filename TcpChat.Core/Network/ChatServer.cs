using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;
using TcpChat.Core.Interfaces;

namespace TcpChat.Core.Network;

public class ChatServer
{
    private readonly IHandlersCollection _handlers;
    private readonly CancellationToken _cancellationToken;
    private readonly TcpListener _listener;

    public ChatServer(IPAddress ip, int port, IHandlersCollection handlers, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _handlers = handlers;
        _listener = new TcpListener(ip, port);
    }

    public void Start()
    {
        try
        {
            _listener.Start(10);

            Console.WriteLine("Server started");

            while (!_cancellationToken.IsCancellationRequested)
            {
                var client = _listener.AcceptTcpClient();
                AcceptClient(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _listener.Stop();
            Console.WriteLine("Server stopped");
        }
    }
    
    private void AcceptClient(TcpClient client)
    {
        Console.WriteLine($"Connection from {client.Client.RemoteEndPoint}");

        Task.Run(() => ReceivePacketsAsync(client), _cancellationToken);
    }

    private async void ReceivePacketsAsync(TcpClient tcpClient)
    {   
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var request = JsonSerializer.Deserialize<Packet>(tcpClient.GetStream());

                if (request is null)
                    continue;

                Console.WriteLine($"Received from {tcpClient.Client.RemoteEndPoint}");

                var handler = _handlers.Resolve(request.Event);

                if (handler is null)
                    continue;

                await handler.HandleAsync(request, tcpClient, _cancellationToken);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Disconnected {tcpClient.Client.RemoteEndPoint}");
        }
        finally
        {
            tcpClient.Close();
        }
    }
}