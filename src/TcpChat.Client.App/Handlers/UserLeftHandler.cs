using Spectre.Console;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Client.App.Handlers;

public class UserLeftHandler : PacketHandler<UserLeftMessage>
{
    private readonly IUserStorage _userStorage;

    public UserLeftHandler(IEncoder<Packet> packetEncoder, IUserStorage userStorage) : base(packetEncoder)
    {
        _userStorage = userStorage;
    }

    public override Task HandleAsync(UserLeftMessage message, CancellationToken ct = default)
    {
        if (_userStorage.CurrentUser?.CurrentChat?.Id == message.Chat)
            AnsiConsole.MarkupLine(!message.Disconnected
                ? $"[red]<{message.User}> left the chat[/]"
                : $"[red]<{message.User}> disconnected[/]");
        
        return Task.CompletedTask;
    }
}