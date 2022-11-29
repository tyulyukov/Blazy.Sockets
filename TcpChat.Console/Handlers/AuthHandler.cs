using System.Net;
using TcpChat.Console.Models;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class AuthHandler : PacketHandler<string>
{
    private readonly ILogHandler _logger;

    public AuthHandler(ILogHandler logger, IEncoder<Packet> packetEncoder) : base(packetEncoder)
    {
        _logger = logger;
    }

    public override async Task HandleAsync(string request, CancellationToken ct)
    {
        var user = new User()
        {
            Name = request,
            EndPoint = Sender.RemoteEndPoint ?? Sender.LocalEndPoint
        };
        
        _logger.HandleText($"Message received from {Sender.RemoteEndPoint}: {request}");
        await SendResponseAsync(new Packet
        {
            Event = "Message",
            State = $"This is your message: {request}"
        }, ct);
    }
}