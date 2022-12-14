using Autofac;
using Blazy.Sockets.Encoding;
using Blazy.Sockets.Network;
using Blazy.Sockets.Sample.Client;
using Blazy.Sockets.Sample.Client.Executables;
using Blazy.Sockets.Sample.Client.Handlers;
using Blazy.Sockets.Sample.Client.Services;
using Spectre.Console;

var builder = new NetworkBuilder();
builder.UseDefaultEncoder();

builder.Use<IServerCommandParserService, ServerCommandParserService>();
builder.Use<IUserStorage, UserStorage>();

builder.Use<ConnectToServerExecutable>();
builder.Use<ConnectToChatExecutable>();
builder.Use<ChatExecutable>();
builder.Use<CreateMyChatExecutable>();

builder.UsePacketHandler<MessageHandler>("Message");
builder.UsePacketHandler<UserJoinedHandler>("User Joined");
builder.UsePacketHandler<UserLeftHandler>("User Left");

await using var app = builder.Build();

using var cts = new CancellationTokenSource();

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs) => cts.Cancel();

Console.CancelKeyPress += ConsoleOnCancelKeyPress;

while (!cts.Token.IsCancellationRequested)
{
    try
    {
        AnsiConsoleUtil.ClearToBeginning();
        
        var executable = app.Resolve<ConnectToServerExecutable>();
        await executable.ExecuteAsync(cts.Token);
    }
    catch (Exception exception)
    {
        AnsiConsole.WriteException(exception);
    }
    finally
    {
        if (!AnsiConsole.Confirm("Do you wanna [green]restart[/]?"))
            cts.Cancel();
    }
}

Console.CancelKeyPress -= ConsoleOnCancelKeyPress;
