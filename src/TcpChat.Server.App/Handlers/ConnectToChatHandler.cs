using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Server.App.Dto;
using TcpChat.Server.App.Services;

namespace TcpChat.Server.App.Handlers;

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
        var sender = _authService.FindBySocket(Sender);

        if (sender is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }

        var chat = _chatService.JoinChat(request.Id, sender);
        
        if (chat is null)
        {
            await SendErrorAsync("An error occured during joining chat", ct);
            return;
        }

        _logger.HandleText($"{sender.Name} connected to chat {chat.Name} with id {request.Id}");
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

        foreach (var user in _chatService.GetUsersFromChat(request.Id, u => u.Name != sender.Name))
        {
            await SendResponseAsync(user.Socket, new Packet
            {
                Event = "User Joined",
                State = new
                {
                    User = sender.Name,
                    Chat = request.Id
                }
            }, ct);
        }
    }
}