using System.Text.Json;
using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class CreateChatHandler : PacketHandler
{
    private readonly ILogHandler _logger;
    private readonly IChatService _chatService;

    public CreateChatHandler(IEncoder<Packet> packetEncoder, IChatService chatService, ILogHandler logger) : base(packetEncoder)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task HandleAsync(Packet packet, CancellationToken ct)
    {
        var chat = JsonDocument.Parse(packet.State.ToString() ?? string.Empty).Deserialize<Chat>(); // TODO get rid of this sh1t

        if (chat is null)
        {
            await SendErrorAsync("Bad request", ct);
            return;
        }
        
        var id = _chatService.CreateChat(chat);
        _logger.HandleText($"Chat {id} was created by {Sender.RemoteEndPoint}");
        await SendResponseAsync(new Packet
        {
            Event = "Chat Created",
            State = id
        }, ct);
    }
}