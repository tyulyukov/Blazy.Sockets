using TcpChat.Console.Dto;
using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class SendMessageHandler : PacketHandler<SendMessageRequest>
{
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;
    private readonly ILogHandler _logger;

    public SendMessageHandler(IEncoder<Packet> packetEncoder, IAuthService authService, IChatService chatService, ILogHandler logger) : base(packetEncoder)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(SendMessageRequest request, CancellationToken ct)
    {
        var sender = _authService.FindBySocket(Sender);

        if (sender is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }
        
        var users = _chatService.GetUsersFromChat(request.Chat, user => user.Name != sender.Name);

        foreach (var user in users)
        {
            await SendResponseAsync(user.Socket, new Packet()
            {
                Event = "Message",
                State = new
                {
                    From = user.Name,
                    request.Chat,
                    request.Message
                }
            }, ct);
        }
        
        _logger.HandleText($"{sender.Name} - {Sender.RemoteEndPoint} sent message {request.Message} to chat {request.Chat}");
    }
}