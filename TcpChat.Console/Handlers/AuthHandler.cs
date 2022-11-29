using System.Net;
using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class AuthHandler : PacketHandler<string>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;

    public AuthHandler(ILogHandler logger, IEncoder<Packet> packetEncoder, IAuthService authService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
    }

    public override async Task HandleAsync(string request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request,
            Socket = Sender
        };

        var result = _authService.Authenticate(user);

        if (!result)
        {
            await SendErrorAsync("Username is already taken", ct);
            return;
        }
        
        _logger.HandleText($"{Sender.RemoteEndPoint} authenticated as {user.Name}");
        await SendResponseAsync(new Packet
        {
            Event = "Authenticated",
            State = $"Authenticated as {user.Name}"
        }, ct);
    }
}