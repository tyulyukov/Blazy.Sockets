using System.Net.Sockets;
using TcpChat.Core.Contracts;
using TcpChat.Core.Interfaces;

namespace TcpChat.Console;

public class MessageHandler : IPacketHandler
{
    private readonly ILogHandler _logger;

    public MessageHandler(ILogHandler logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(Packet packet, Socket sender, CancellationToken ct)
    {
        _logger.HandleText($"Message received from {sender.RemoteEndPoint}: {packet.State}");
        return Task.CompletedTask;
    }
}