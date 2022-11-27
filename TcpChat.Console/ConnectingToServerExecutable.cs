using System.Net;
using Spectre.Console;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;

namespace TcpChat.Console;

public class ConnectingToServerExecutable : IExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly IEncoder<Packet> _packetEncoder;

    private string? ip;
    private int? port;
    
    public ConnectingToServerExecutable(IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (ip is null || port is null)
            throw new ApplicationException("Executable is not configured");
        
        using var client = new ChatClient(IPAddress.Parse(ip), port.Value, _packetEncoder);
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .StartAsync("Connecting", async ctx =>
            {
                await client.ConnectAsync(token);
                AnsiConsole.WriteLine("Connected to the server");
            });

        try
        {
            while (!token.IsCancellationRequested)
            {
                var message = AnsiConsole.Ask<string>("Send a [yellow]message[/]");
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.SimpleDotsScrolling)
                    .StartAsync("Sending message", async ctx =>
                    {
                        await client.SendRequestAsync(new Packet { Event = "Message", State = message }, token);
                        AnsiConsole.WriteLine("Message sent successfully");

                        ctx.Status("Receiving response");

                        var response = await client.ReceiveResponseAsync(token);

                        if (response is null)
                            AnsiConsole.WriteLine("Without response");
                        else
                            AnsiConsole.WriteLine("Response from server: " + response.State);
                    });
            }
        }
        catch (SocketDisconnectedException exception)
        {
            AnsiConsole.WriteLine("Disconnected from server");
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
        }
        finally
        {
            AnsiConsole.WriteLine("Execution stopped");
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