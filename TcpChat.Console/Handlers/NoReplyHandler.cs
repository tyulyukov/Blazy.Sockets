using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class NoReplyHandler : PacketHandler
{
    private readonly ILogHandler _logger;

    public NoReplyHandler(ILogHandler logger, IEncoder<Packet> packetEncoder) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override Task HandleAsync(Packet packet, CancellationToken ct)
    {
        _logger.HandleText($"No reply message received from {Sender.RemoteEndPoint}: {packet.State}");
        return Task.CompletedTask;
    }
}