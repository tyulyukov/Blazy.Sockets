using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using TcpChat.Core.Logging;

namespace TcpChat.Core.Network;

internal class ChatServer : INetworkServer
{
    private readonly Socket _listener;
    private readonly IPEndPoint _ipEndPoint;
    private readonly ISocketAcceptor _socketAcceptor;
    private readonly ILogHandler _logger;

    public ChatServer(IConfiguration configuration, ISocketAcceptor socketAcceptor, ILogHandler logger)
    {
        var ip = IPAddress.Parse(configuration.GetValue<string>("Connection:IPAddress"));
        var port = configuration.GetValue<int>("Connection:Port");
        
        _ipEndPoint = new IPEndPoint(ip, port);
        _socketAcceptor = socketAcceptor;
        _logger = logger;
        _listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        try
        {
            _listener.Bind(_ipEndPoint);
            _listener.Listen(100);

            _logger.HandleText($"Server is running on {_ipEndPoint.Port} port");

            while (!ct.IsCancellationRequested)
            {
                var client = await _listener.AcceptAsync(ct);
                await _socketAcceptor.AcceptSocketAsync(client, ct);
            }
        }
        catch (Exception ex)
        {
            if (ex is not OperationCanceledException && ex is not TaskCanceledException)
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