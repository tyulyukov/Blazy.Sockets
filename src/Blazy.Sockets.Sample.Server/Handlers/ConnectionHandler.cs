using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class ConnectionHandler : PacketHandler<ConnectionDetails>
{
    private readonly ILogger _logger;

    public ConnectionHandler(IEncoder<Packet> packetEncoder, ILogger logger) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override Task HandleAsync(ConnectionDetails details, CancellationToken ct)
    {
        _logger.Information("Connection from {RemoteEndPoint}", Sender.RemoteEndPoint);
        return Task.CompletedTask;
    }
}