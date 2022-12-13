using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Sample.Client.Models;
using Blazy.Sockets.Sample.Client.Services;
using Spectre.Console;

namespace Blazy.Sockets.Sample.Client.Handlers;

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