using System.Net;
using System.Net.Sockets;
using Spectre.Console;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables;

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
        
        AnsiConsole.Write(new Rule("[yellow]Chat[/]").LeftJustified());
        
        using var client = new ChatClient(IPAddress.Parse(ip), port.Value, _packetEncoder);
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .StartAsync("Connecting", async ctx =>
            {
                try
                {
                    await client.ConnectAsync(token);
                    AnsiConsole.WriteLine("Connected to the server");
                }
                catch (SocketException exception)
                {
                    AnsiConsole.WriteLine("An error has occurred while connecting with server");
                }
            });

        try
        {
            while (!token.IsCancellationRequested && client.Connected)
            {
                var message = AnsiConsole.Ask<string>("[blue]> Send a message[/]");
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
        AnsiConsole.Write(new Rule("[yellow]Setup[/]").LeftJustified());
        
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