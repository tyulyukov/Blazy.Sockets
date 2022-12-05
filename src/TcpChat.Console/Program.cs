using HashidsNet;
using Spectre.Console;
using TcpChat.Console;
using TcpChat.Console.Domain;
using TcpChat.Console.Executables;
using TcpChat.Console.Executables.Client;
using TcpChat.Console.Executables.Server;
using TcpChat.Console.Handlers;
using TcpChat.Console.Services;
using TcpChat.Core.Handlers;

var logger = new LogHandler();
var handlers = new HandlersCollection();
var encoder = new JsonPacketEncoder();

var hashids = new Hashids(DateTime.UtcNow.ToString(), 5);

var chatService = new ChatService(hashids);
var authService = new AuthService(); 

handlers.Register("Create Chat", new CreateChatHandler(encoder, chatService, authService, logger));
handlers.Register("Connect To Chat", new ConnectToChatHandler(encoder, authService, chatService, logger));
handlers.Register("Leave Chat", new LeaveChatHandler(encoder, authService, chatService, logger));
handlers.Register("Auth", new AuthHandler(logger, encoder, authService));
handlers.Register("Message", new SendMessageHandler(encoder, authService, chatService, logger));
handlers.RegisterConnectionHandler(new ConnectionHandler(encoder, logger));
handlers.RegisterDisconnectionHandler(new DisconnectionHandler(encoder, logger, authService, chatService));

var executables = new IExecutable[]
{
    new HostingServerExecutable(logger, handlers, encoder),
    new ConnectingToServerExecutable(encoder, new ServerCommandParserService())
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
        AnsiConsoleUtil.ClearToBeginning();

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
