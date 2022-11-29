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

    public CreateChatHandler(IEncoder<Packet> packetEncoder, IChatService chatService, ILogHandler logger) : base(packetEncoder)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(Chat request, CancellationToken ct)
    {
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