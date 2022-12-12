using Spectre.Console;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Client.App.Handlers;

public class UserJoinedHandler : PacketHandler<UserJoinedMessage>
{
    private readonly IUserStorage _userStorage;

    public UserJoinedHandler(IEncoder<Packet> packetEncoder, IUserStorage userStorage) : base(packetEncoder)
    {
        _userStorage = userStorage;
    }

    public override Task HandleAsync(UserJoinedMessage message, CancellationToken ct = default)
    {
        if (_userStorage.CurrentUser?.CurrentChat?.Id == message.Chat)
            AnsiConsole.MarkupLine($"[green]<{message.User}> joined the chat[/]");
        
        return Task.CompletedTask;
    }
}