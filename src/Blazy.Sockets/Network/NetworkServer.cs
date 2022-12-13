using System.Net;
using System.Net.Sockets;
using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Microsoft.Extensions.Configuration;

namespace Blazy.Sockets.Network;

internal class NetworkServer : INetworkServer
{
    private readonly Socket _listener;
    private readonly IPEndPoint _ipEndPoint;
    private readonly ISocketAcceptor _socketAcceptor;
    private readonly ILogHandler _logger;
    private readonly IEncoder<Packet> _packetEncoder;

    public NetworkServer(IConfiguration configuration, ISocketAcceptor socketAcceptor, ILogHandler logger, 
        IEncoder<Packet> packetEncoder)
    {
        var ip = IPAddress.Parse(configuration.GetValue<string>("Connection:IPAddress"));
        var port = configuration.GetValue<int>("Connection:Port");
        
        _ipEndPoint = new IPEndPoint(ip, port);
        _socketAcceptor = socketAcceptor;
        _logger = logger;
        _packetEncoder = packetEncoder;
        _listener = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public NetworkServer(IPEndPoint endPoint, ISocketAcceptor socketAcceptor, ILogHandler logger, 
        IEncoder<Packet> packetEncoder)
    {
        _ipEndPoint = endPoint;
        _socketAcceptor = socketAcceptor;
        _logger = logger;
        _packetEncoder = packetEncoder;
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
                var client = new NetworkClient(await _listener.AcceptAsync(ct), _packetEncoder);
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