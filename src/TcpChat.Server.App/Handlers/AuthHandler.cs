using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Server.App.Dto;
using TcpChat.Server.App.Models;
using TcpChat.Server.App.Services;

namespace TcpChat.Server.App.Handlers;

public class AuthHandler : PacketHandler<AuthRequest>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;

    public AuthHandler(ILogHandler logger, IEncoder<Packet> packetEncoder, IAuthService authService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
    }

    public override async Task HandleAsync(AuthRequest request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Username,
            Client = Sender
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