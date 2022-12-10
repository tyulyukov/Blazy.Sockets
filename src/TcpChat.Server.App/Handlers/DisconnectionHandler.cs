using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Server.App.Dto;
using TcpChat.Server.App.Services;

namespace TcpChat.Server.App.Handlers;

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

    public override async Task HandleAsync(DisconnectionDetails details, CancellationToken ct)
    {
        var sender = _authService.FindBySocket(Sender);
        
        if (sender is not null)
        {
            _authService.LogOut(sender.Name);
            
            var leftChats = _chatService.LeaveAllChats(sender);

            foreach (var leftChat in leftChats)
            {
                foreach (var user in _chatService.GetUsersFromChat(leftChat.HashId, u => u.Name != sender.Name))
                {
                    await SendResponseAsync(user.Socket, new Packet
                    {
                        Event = "User Left",
                        State = new UserLeftChatDto
                        {
                            User = sender.Name,
                            Chat = leftChat.HashId,
                            Disconnected = true
                        }
                    }, ct);
                }
            }
            
            switch (leftChats.Count)
            {
                case 0:
                    _logger.HandleText($"{sender.Name} was not connected to any chat while disconnecting");
                    break;
                case 1:
                    _logger.HandleText($"{sender.Name} left {leftChats[0].Name} chat due to disconnection");
                    break;
                case > 1:
                    _logger.HandleText($"{sender.Name} left {leftChats.Count} chats due to disconnection");
                    break;
            }
            
            _logger.HandleText($"Disconnected user {sender.Name} Connection Elapsed: {details.ConnectionTime}");
        }
        else
        {
            _logger.HandleText($"Disconnected {Sender.RemoteEndPoint} Connection Elapsed: {details.ConnectionTime}");
        }
    }
}