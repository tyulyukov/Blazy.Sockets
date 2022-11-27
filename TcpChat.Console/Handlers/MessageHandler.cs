using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class MessageHandler : PacketHandler
{
    private readonly ILogHandler _logger;

    public MessageHandler(ILogHandler logger, IEncoder<Packet> packetEncoder) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override async Task HandleAsync(Packet packet, CancellationToken ct)
    {
        _logger.HandleText($"Message received from {Sender.RemoteEndPoint}: {packet.State}");
        await SendResponseAsync(new Packet
        {
            Event = packet.Event,
            State = $"This is your message: {packet.State}"
        }, ct);
    }
}