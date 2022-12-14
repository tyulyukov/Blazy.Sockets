using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class LeaveChatHandler : PacketHandler<LeaveChatRequest>
{
    private readonly ILogger _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public LeaveChatHandler(IAuthService authService, IChatService chatService, ILogger logger)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(LeaveChatRequest request, CancellationToken ct)
    {
        var sender = _authService.FindBySender(Sender);

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

        _logger.Information("{Username} has left chat {ChatId}", sender.Name, request.Id);
        await SendResponseAsync(new Packet()
        {
            Event = "Left Chat",
            State = request.Id
        }, ct);
        
        foreach (var user in _chatService.GetUsersFromChat(request.Id, u => u.Name != sender.Name))
        {
            await SendResponseAsync(user.Client, new Packet
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