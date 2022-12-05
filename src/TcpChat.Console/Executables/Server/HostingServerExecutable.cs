using System.Net;
using Spectre.Console;
using TcpChat.Console.Domain;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables.Server;

public class HostingServerExecutable : IConfigurableExecutable
{
    public string RepresentationText => "Host a server";

    private readonly ILogHandler _logger;
    private readonly IHandlersCollection _handlers;
    private readonly IEncoder<Packet> _packetEncoder;

    private int? _port;
    
    public HostingServerExecutable(ILogHandler logger, IHandlersCollection handlers, IEncoder<Packet> packetEncoder)
    {
        _logger = logger;
        _handlers = handlers;
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (_port is null)
            throw new ApplicationException("Executable is not configured");

        AnsiConsole.Write(new Rule("[yellow]Logs[/]").LeftJustified());

        using var socketAcceptor = new SocketAcceptor(_handlers, _packetEncoder, _logger);
        using var server = new ChatServer(new IPEndPoint(IPAddress.Any, _port.Value), socketAcceptor, _logger);

        await server.RunAsync(token);
    }

    public void Configure()
    {
        AnsiConsole.Write(new Rule("[yellow]Configuration[/]").LeftJustified());
        
        _port = AnsiConsole.Prompt(
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