using System.Net;
using Spectre.Console;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;

namespace TcpChat.Console;

public class ConnectingToServerExecutable : IExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly ILogHandler _logger;
    private readonly IEncoder<Packet> _packetEncoder;

    private string? ip;
    private int? port;
    
    public ConnectingToServerExecutable(ILogHandler logger, IEncoder<Packet> packetEncoder)
    {
        _logger = logger;
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (ip is null || port is null)
            throw new ApplicationException("Executable is not configured");
        
        using var client = new ChatClient(IPAddress.Parse(ip), port.Value, _logger, _packetEncoder);
        await client.ConnectAsync(token);

        while (!token.IsCancellationRequested)
        {
            var message = AnsiConsole.Ask<string>("Send a [yellow]message[/]");
            var response = await client.SendAsync(new Packet { Event = "Message", State = message }, token);
            
            if (response is null)
                _logger.HandleText("Without response");
            else
                _logger.HandleText("Response from server: " + response.State);
        }
    }

    public void Configure()
    {
        ip = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]ip address[/] of the server u wanna connect to")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid ip address[/]")
                .Validate(str => IPAddress.TryParse(str, out _) ? ValidationResult.Success() : ValidationResult.Error()));
        
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