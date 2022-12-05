using TcpChat.Console.Dto;
using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

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
        var user = _authService.FindBySocket(Sender);

        if (user is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }

        if (!_chatService.LeaveChat(request.Id, user))
        {
            await SendErrorAsync("An error occured during leaving chat", ct);
            return;
        }

        _logger.HandleText($"{user.Name} has left chat {request.Id}");
        await SendResponseAsync(new Packet()
        {
            Event = "Left Chat",
            State = request.Id
        }, ct);
    }
}