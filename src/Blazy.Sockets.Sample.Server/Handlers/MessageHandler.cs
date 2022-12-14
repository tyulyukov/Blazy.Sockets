﻿using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class SendMessageHandler : PacketHandler<SendMessageRequest>
{
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;
    private readonly ILogger _logger;

    public SendMessageHandler(IAuthService authService, IChatService chatService, ILogger logger)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(SendMessageRequest request, CancellationToken ct)
    {
        var sender = _authService.FindBySender(Sender);

        if (sender is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }
        
        var users = _chatService.GetUsersFromChat(request.Chat, user => user.Name != sender.Name);

        foreach (var user in users)
        {
            await SendResponseAsync(user.Client, new Packet()
            {
                Event = "Message",
                State = new
                {
                    From = sender.Name,
                    request.Chat,
                    request.Message
                }
            }, ct);
        }
        
        _logger.Information("{Username} sent message {Message} to chat {ChatId}", sender.Name, request.Message, request.Chat);
    }
}