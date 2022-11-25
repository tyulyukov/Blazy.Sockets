using System.Net;
using Spectre.Console;
using TcpChat.Core;
using TcpChat.Core.Interfaces;
using TcpChat.Core.Network;

namespace TcpChat.Console;

public class HostingServerExecutable : IExecutable
{
    public string RepresentationText => "Host a server";

    private readonly ILogHandler _logger;
    private readonly IHandlersCollection _handlers;
    
    private int? port;
    
    public HostingServerExecutable(ILogHandler logger, IHandlersCollection handlers)
    {
        _logger = logger;
        _handlers = handlers;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (port is null)
            throw new ApplicationException("Executable is not configured");

        using var server = new ChatServer(new IPEndPoint(IPAddress.Any, port.Value), _handlers, token, _logger);

        var task = Task.Run(server.Run, token);

        await task.WaitAsync(token);
    }

    public void Configure()
    {
        port = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter [green]port[/]")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid port[/]")
                .Validate(num =>
                {
                    return num switch
                    {
                        >= 1 and <= 65_535 => ValidationResult.Success(),
                        _ => ValidationResult.Error(),
                    };
                }));
    }
}