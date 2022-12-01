using HashidsNet;
using Spectre.Console;
using TcpChat.Console.Executables;
using TcpChat.Console.Handlers;
using TcpChat.Console.Services;
using TcpChat.Core.Handlers;

var logger = new LogHandler();
var handlers = new HandlersCollection();
var encoder = new JsonPacketEncoder();

char GetRandomLetter()
{
    const string chars = "$%#@!*;:?^&abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    var num = Random.Shared.Next(0, chars.Length);
    return chars[num];
}

var hashids = new Hashids(Enumerable.Range(0, 10).Select(_ => GetRandomLetter()).ToString(), 5);

var chatService = new ChatService(hashids);
var authService = new AuthService(); 

handlers.Register("Create Chat", new CreateChatHandler(encoder, chatService, logger));
handlers.Register("Auth", new AuthHandler(logger, encoder, authService));
handlers.RegisterConnectionHandler(new ConnectionHandler(encoder, logger));
handlers.RegisterDisconnectionHandler(new DisconnectionHandler(encoder, logger, authService));

var executables = new IExecutable[]
{
    new HostingServerExecutable(logger, handlers, encoder),
    new ConnectingToServerExecutable(encoder)
};

using var cts = new CancellationTokenSource();
var token = cts.Token;

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
{
    cts.Cancel();
}

Console.CancelKeyPress += ConsoleOnCancelKeyPress;

while (!token.IsCancellationRequested)
{
    try
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("WELCOME TO TCP CHAT")
                .Centered()
                .Color(Color.Teal));

        var executable = AnsiConsole.Prompt(
            new SelectionPrompt<IExecutable>()
                .Title("What you [green]gonna do[/]?")
                .UseConverter(exe => exe.RepresentationText)
                .AddChoices(executables));

        if (executable is IConfigurableExecutable exe)
            exe.Configure();

        await executable.ExecuteAsync(token);
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
