using Autofac;
using Spectre.Console;
using TcpChat.Client.App;
using TcpChat.Client.App.Executables;
using TcpChat.Client.App.Services;
using TcpChat.Core.Application;

var builder = new NetworkBuilder();

builder.Use<IServerCommandParserService, ServerCommandParserService>();

builder.Use<ConnectToServerExecutable>();
builder.Use<ConnectToChatExecutable>();
builder.Use<ChatExecutable>();
builder.Use<CreateMyChatExecutable>();

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
