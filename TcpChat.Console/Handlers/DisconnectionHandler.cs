using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class DisconnectionHandler : PacketHandler<DisconnectionDetails>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;

    public DisconnectionHandler(IEncoder<Packet> packetEncoder, ILogHandler logger, IAuthService authService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
    }

    public override Task HandleAsync(DisconnectionDetails details, CancellationToken ct)
    {
        var user = _authService.FindBySocket(Sender);
        
        if (user is not null)
        {
            _authService.LogOut(user.Name);
            _logger.HandleText($"Disconnected user {user.Name} - {Sender.RemoteEndPoint} Connection Elapsed: {details.ConnectionTime}");
        }
        else
        {
            _logger.HandleText($"Disconnected anonymous {Sender.RemoteEndPoint} Connection Elapsed: {details.ConnectionTime}");
        }

        return Task.CompletedTask;
    }
}