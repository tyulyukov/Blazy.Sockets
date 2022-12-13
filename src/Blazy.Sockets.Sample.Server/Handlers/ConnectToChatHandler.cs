using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class ConnectToChatHandler : PacketHandler<ConnectToChatRequest>
{
    private readonly ILogger _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public ConnectToChatHandler(IEncoder<Packet> packetEncoder, IAuthService authService, IChatService chatService, ILogger logger) : base(packetEncoder)
    {
        _authService = authService;
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(ConnectToChatRequest request, CancellationToken ct)
    {
        var sender = _authService.FindBySender(Sender);

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

        _logger.Information("{Username} connected to chat {ChatName} with id {ChatId}", sender.Name, chat.Name, request.Id);
        await SendResponseAsync(new Packet()
        {
            Event = "Connected To Chat",
            State = new
            {
                chat.Name,
                Creator = new
                {
                    chat.Creator.Name
                }
            }
        }, ct);

        foreach (var user in _chatService.GetUsersFromChat(request.Id, u => u.Name != sender.Name))
        {
            await SendResponseAsync(user.Client, new Packet
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