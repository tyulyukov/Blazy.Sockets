using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Sample.Client.Models;
using Blazy.Sockets.Sample.Client.Services;
using Spectre.Console;

namespace Blazy.Sockets.Sample.Client.Handlers;

public class MessageHandler : PacketHandler<Message>
{
    private readonly IUserStorage _userStorage;

    public MessageHandler(IUserStorage userStorage)
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