using System.Net;
using Spectre.Console;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;

namespace TcpChat.Console;

public class HostingServerExecutable : IExecutable
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

        using var server = new ChatServer(new IPEndPoint(IPAddress.Any, port.Value), _handlers, _logger, _packetEncoder);

        var task = Task.Run(() => server.RunAsync(token), token);

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