using Spectre.Console;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Client.App.Handlers;

public class MessageHandler : PacketHandler<Message>
{
    private readonly IUserStorage _userStorage;

    public MessageHandler(IEncoder<Packet> packetEncoder, IUserStorage userStorage) : base(packetEncoder)
    {
        _userStorage = userStorage;
    }

    public override Task HandleAsync(Message message, CancellationToken ct = default)
    {
        if (_userStorage.CurrentUser?.CurrentChat?.Id == message.Chat)
            AnsiConsole.WriteLine($"<{message.From}> {message.Content}");
        
        return Task.CompletedTask;
    }
}