using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class DisconnectionHandler : PacketHandler<DisconnectionDetails>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public DisconnectionHandler(IEncoder<Packet> packetEncoder, ILogHandler logger, IAuthService authService, IChatService chatService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
        _chatService = chatService;
    }

    public override Task HandleAsync(DisconnectionDetails details, CancellationToken ct)
    {
        var user = _authService.FindBySocket(Sender);
        
        if (user is not null)
        {
            _authService.LogOut(user.Name);
            
            var leftChats = _chatService.LeaveAllChats(user);
            
            if (leftChats.Count == 0)
                _logger.HandleText($"{user.Name} was not connected to any chat disconnection");
            else if (leftChats.Count == 1)
                _logger.HandleText($"{user.Name} left {leftChats[0].Name} chat due to disconnection");
            else if (leftChats.Count > 1)
                _logger.HandleText($"{user.Name} left {leftChats.Count} chats due to disconnection");
            
            _logger.HandleText($"Disconnected user {user.Name} Connection Elapsed: {details.ConnectionTime}");
        }
        else
        {
            _logger.HandleText($"Disconnected {Sender.RemoteEndPoint} Connection Elapsed: {details.ConnectionTime}");
        }

        return Task.CompletedTask;
    }
}