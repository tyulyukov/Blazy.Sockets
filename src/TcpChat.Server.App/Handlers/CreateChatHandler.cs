using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Server.App.Dto;
using TcpChat.Server.App.Models;
using TcpChat.Server.App.Services;

namespace TcpChat.Server.App.Handlers;

public class CreateChatHandler : PacketHandler<CreateChatRequest>
{
    private readonly ILogHandler _logger;
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;

    public CreateChatHandler(IEncoder<Packet> packetEncoder, IChatService chatService, IAuthService authService, ILogHandler logger) : base(packetEncoder)
    {
        _chatService = chatService;
        _authService = authService;
        _logger = logger;
    }

    public override async Task HandleAsync(CreateChatRequest request, CancellationToken ct)
    {
        var user = _authService.FindBySender(Sender);

        if (user is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }

        var chat = new Chat
        {
            Name = request.Name,
            Creator = user
        };
        chat.Users.Add(user);
        
        var id = _chatService.CreateChat(chat);

        if (id is null)
        {
            await SendErrorAsync("Internal Error", ct);
            return;
        }
        
        _logger.HandleText($"Chat {chat.Name} with id {id} was created by {user.Name}");
        await SendResponseAsync(new Packet
        {
            Event = "Chat Created",
            State = id
        }, ct);
    }
}