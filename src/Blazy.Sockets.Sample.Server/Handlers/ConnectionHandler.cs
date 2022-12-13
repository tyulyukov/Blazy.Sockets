using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;

namespace Blazy.Sockets.Sample.Server.Handlers;

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