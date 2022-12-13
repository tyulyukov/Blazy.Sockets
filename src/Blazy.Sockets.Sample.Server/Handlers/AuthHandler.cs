using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Models;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class AuthHandler : PacketHandler<AuthRequest>
{
    private readonly ILogger _logger;
    private readonly IAuthService _authService;

    public AuthHandler(ILogger logger, IEncoder<Packet> packetEncoder, IAuthService authService) : base(packetEncoder)
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
        
        _logger.Information("{RemoteEndPoint} authenticated as {Username}", Sender.RemoteEndPoint, user.Name);
        await SendResponseAsync(new Packet
        {
            Event = "Authenticated",
            State = $"Authenticated as {user.Name}"
        }, ct);
    }
}