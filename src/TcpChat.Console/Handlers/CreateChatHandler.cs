using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class CreateChatHandler : PacketHandler<Chat>
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

    public override async Task HandleAsync(Chat request, CancellationToken ct)
    {
        var user = _authService.FindBySocket(Sender);

        if (user is null)
        {
            await SendErrorAsync("Not authenticated", ct);
            return;
        }
        
        request.Users.Add(user);
        
        var id = _chatService.CreateChat(request);

        if (id is null)
        {
            await SendErrorAsync("Internal Error", ct);
            return;
        }
        
        _logger.HandleText($"Chat {id} was created by {Sender.RemoteEndPoint}");
        await SendResponseAsync(new Packet
        {
            Event = "Chat Created",
            State = id
        }, ct);
    }
}