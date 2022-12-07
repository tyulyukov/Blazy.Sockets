using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Server.App.Handlers;

public class ConnectionHandler : PacketHandler<ConnectionDetails>
{
    private readonly ILogHandler _logger;

    public ConnectionHandler(IEncoder<Packet> packetEncoder, ILogHandler logger) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override Task HandleAsync(ConnectionDetails details, CancellationToken ct)
    {
        _logger.HandleText($"Connection from {Sender.RemoteEndPoint}");
        return Task.CompletedTask;
    }
}