using System.Net;
using Spectre.Console;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables;

public class HostingServerExecutable : IConfigurableExecutable
{
    public string RepresentationText => "Host a server";

    private readonly ILogHandler _logger;
    private readonly IHandlersCollection _handlers;
    private readonly IEncoder<Packet> _packetEncoder;

    private int? port;
    
    public HostingServerExecutable(ILogHandler logger, IHandlersCollection handlers, IEncoder<Packet> packetEncoder)
    {
        _logger = logger;
        _handlers = handlers;
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (port is null)
            throw new ApplicationException("Executable is not configured");

        AnsiConsole.Write(new Rule("[yellow]Logs[/]").LeftJustified());
        
        using var server = new ChatServer(new IPEndPoint(IPAddress.Any, port.Value), _handlers, _logger, _packetEncoder);

        await server.RunAsync(token);
    }

    public void Configure()
    {
        AnsiConsole.Write(new Rule("[yellow]Configuration[/]").LeftJustified());
        
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