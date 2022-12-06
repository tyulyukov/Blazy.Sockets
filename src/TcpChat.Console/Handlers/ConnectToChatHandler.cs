using TcpChat.Console.Dto;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class ConnectToChatHandler : PacketHandler<ConnectToChatRequest>
{
    private readonly ILogHandler _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public ConnectToChatHandler(IEncoder<Packet> packetEncoder, IAuthService authService, IChatService chatService, ILogHandler logger) : base(packetEncoder)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(ConnectToChatRequest request, CancellationToken ct)
    {
        var user = _authService.FindBySocket(Sender);

        if (user is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }

        var chat = _chatService.JoinChat(request.Id, user);
        
        if (chat is null)
        {
            await SendErrorAsync("An error occured during joining chat", ct);
            return;
        }

        _logger.HandleText($"{user.Name} connected to chat {chat.Name} with id {request.Id}");
        await SendResponseAsync(new Packet()
        {
            Event = "Connected To Chat",
            State = new
            {
                Name = chat.Name,
                Creator = new
                {
                    Name = chat.Creator.Name
                }
            }
        }, ct);
    }
}