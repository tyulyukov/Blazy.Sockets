using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Server.App.Dto;
using TcpChat.Server.App.Services;

namespace TcpChat.Server.App.Handlers;

public class LeaveChatHandler : PacketHandler<LeaveChatRequest>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public LeaveChatHandler(IEncoder<Packet> packetEncoder, IAuthService authService, IChatService chatService, ILogHandler logger) : base(packetEncoder)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(LeaveChatRequest request, CancellationToken ct)
    {
        var sender = _authService.FindBySocket(Sender);

        if (sender is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }

        if (!_chatService.LeaveChat(request.Id, sender))
        {
            await SendErrorAsync("An error occured during leaving chat", ct);
            return;
        }

        _logger.HandleText($"{sender.Name} has left chat {request.Id}");
        await SendResponseAsync(new Packet()
        {
            Event = "Left Chat",
            State = request.Id
        }, ct);
        
        foreach (var user in _chatService.GetUsersFromChat(request.Id, u => u.Name != sender.Name))
        {
            await SendResponseAsync(user.Socket, new Packet
            {
                Event = "User Left",
                State = new UserLeftChatDto
                {
                    User = sender.Name,
                    Chat = request.Id,
                    Disconnected = false
                }
            }, ct);
        }
    }
}