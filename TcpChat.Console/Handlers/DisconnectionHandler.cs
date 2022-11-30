using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class DisconnectionHandler : PacketHandler<DisconnectionDetails>
{
    private readonly ILogHandler _logger;

    public DisconnectionHandler(IEncoder<Packet> packetEncoder, ILogHandler logger) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override Task HandleAsync(DisconnectionDetails details, CancellationToken ct)
    {
        _logger.HandleText($"Disconnected {Sender.RemoteEndPoint} Connection Elapsed: {details.ConnectionTime}");
        return Task.CompletedTask;
    }
}