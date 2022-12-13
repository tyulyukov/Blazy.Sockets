using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Sample.Client.Models;
using Blazy.Sockets.Sample.Client.Services;
using Spectre.Console;

namespace Blazy.Sockets.Sample.Client.Handlers;

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