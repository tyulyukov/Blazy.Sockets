using Spectre.Console;
using TcpChat.Console;
using TcpChat.Console.Executables;
using TcpChat.Console.Handlers;
using TcpChat.Core.Handlers;

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("WELCOME TO TCP CHAT")
        .Centered()
        .Color(Color.Purple));

var logger = new LogHandler();
var handlers = new HandlersCollection();
var encoder = new JsonPacketEncoder();

handlers.Register("Message", new MessageHandler(logger, encoder));
handlers.Register("No Reply", new NoReplyHandler(logger, encoder));

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
Console.ReadKey();
