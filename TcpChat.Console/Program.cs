using HashidsNet;
using Spectre.Console;
using TcpChat.Console.Executables;
using TcpChat.Console.Handlers;
using TcpChat.Console.Services;
using TcpChat.Core.Handlers;

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("WELCOME TO TCP CHAT")
        .Centered()
        .Color(Color.Teal)); 

var logger = new LogHandler();
var handlers = new HandlersCollection();
var encoder = new JsonPacketEncoder();

var hashids = new Hashids("drip too hard", 5);

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

var executable = AnsiConsole.Prompt(
    new SelectionPrompt<IExecutable>()
        .Title("What you [green]gonna do[/]?")
        .UseConverter(exe => exe.RepresentationText)
        .AddChoices(executables));

using var cts = new CancellationTokenSource();
var token = cts.Token;

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
{
    cts.Cancel();
}

Console.CancelKeyPress += ConsoleOnCancelKeyPress;

try
{
    executable.Configure();
    await executable.ExecuteAsync(token);
}
catch (Exception exception)
{
    AnsiConsole.WriteException(exception);
}

Console.CancelKeyPress -= ConsoleOnCancelKeyPress;
