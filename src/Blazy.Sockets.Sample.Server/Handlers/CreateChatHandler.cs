using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Models;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class CreateChatHandler : PacketHandler<CreateChatRequest>
{
    private readonly ILogger _logger;
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;

    public CreateChatHandler(IChatService chatService, IAuthService authService, ILogger logger)
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
        
        _logger.Information("Chat {ChatName} with id {ChatId} was created by {Username}", chat.Name, id, user.Name);
        await SendResponseAsync(new Packet
        {
            Event = "Chat Created",
            State = id
        }, ct);
    }
}