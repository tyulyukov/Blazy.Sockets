using System.Net;
using System.Net.Sockets;
using TcpChat.Core.Logging;

namespace TcpChat.Core.Network;

public class ChatServer : INetworkServer
{
    private readonly Socket _listener;
    private readonly IPEndPoint _ipEndPoint;
    private readonly ISocketAcceptor _socketAcceptor;
    private readonly ILogHandler _logger;

    public ChatServer(IPEndPoint ipEndPoint, ISocketAcceptor socketAcceptor, ILogHandler logger)
    {
        _ipEndPoint = ipEndPoint;
        _socketAcceptor = socketAcceptor;
        _logger = logger;
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
                await _socketAcceptor.AcceptSocketAsync(client, ct);
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

    public void Dispose()
    {
        _listener.Close();
    }
}