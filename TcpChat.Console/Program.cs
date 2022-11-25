using Spectre.Console;
using TcpChat.Console;
using TcpChat.Core;

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("WELCOME TO TCP CHAT")
        .Centered()
        .Color(Color.Purple));

var logger = new LogHandler();
var handlers = new HandlersCollection();

// handlers.Register("Message", new MessageHandler(logger));

var executables = new IExecutable[]
{
    new HostingServerExecutable(logger, handlers),
    new ConnectingToServerExecutable(logger)
};

var executable = AnsiConsole.Prompt(
    new SelectionPrompt<IExecutable>()
        .Title("What you [green]gonna do[/]?")
        .UseConverter(exe => exe.RepresentationText)
        .AddChoices(executables));

var cts = new CancellationTokenSource();
var token = cts.Token;

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
{
    cts.Cancel();
}

Console.CancelKeyPress += ConsoleOnCancelKeyPress;

executable.Configure();
await executable.ExecuteAsync(token);

Console.CancelKeyPress -= ConsoleOnCancelKeyPress;
