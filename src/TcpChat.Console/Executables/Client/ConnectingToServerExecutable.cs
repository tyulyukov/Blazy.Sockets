using System.Net;
using System.Net.Sockets;
using Spectre.Console;
using TcpChat.Console.Domain;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables.Client;

public class ConnectingToServerExecutable : IConfigurableExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly IEncoder<Packet> _packetEncoder;
    private readonly IServerCommandParserService _commandParserService;

    private string? _ip;
    private int? _port;
    
    public ConnectingToServerExecutable(IEncoder<Packet> packetEncoder, IServerCommandParserService commandParserService)
    {
        _packetEncoder = packetEncoder;
        _commandParserService = commandParserService;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        if (_ip is null || _port is null)
            throw new ApplicationException("Executable is not configured");
        
        AnsiConsole.Write(new Rule("[yellow]Chat[/]").LeftJustified());
        
        using var client = new ChatClient(IPAddress.Parse(_ip), _port.Value, _packetEncoder);
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .StartAsync("Connecting", async ctx =>
            {
                try
                {
                    await client.ConnectAsync(token);
                    AnsiConsole.MarkupLine("Connected to the server");
                }
                catch (SocketException exception)
                {
                    AnsiConsole.MarkupLine("An [red]error[/] has occurred while connecting with server");
                }
            });

        if (token.IsCancellationRequested || !client.Connected)
            return;
        
        try
        {
            var username = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter [green]username[/]")
                    .PromptStyle("green"));

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (await AuthenticateAsync(client, username, token))
                        break;

                    username = AnsiConsole.Prompt(
                        new TextPrompt<string>(
                                $"Username {username} is [red]already taken[/]. Try [green]another one[/]")
                            .PromptStyle("green"));
                }
                catch (SocketDisconnectedException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    AnsiConsole.WriteException(exception);
                }
            }
            
            var executables = new IExecutable[]
            {
                new ConnectToChatExecutable(client, _commandParserService), 
                new CreateMyChatExecutable(client, _commandParserService)
            };

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var executable = AnsiConsole.Prompt(
                        new SelectionPrompt<IExecutable>()
                            .Title($"[yellow]{username}[/], what do u you wanna [green]start with[/]?")
                            .UseConverter(exe => exe.RepresentationText)
                            .AddChoices(executables));

                    if (executable is IConfigurableExecutable configurableExe)
                        configurableExe.Configure();

                    await executable.ExecuteAsync(token);
                }
                catch (SocketDisconnectedException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    AnsiConsole.WriteException(exception);
                }
            }
        }
        catch (SocketDisconnectedException exception)
        {
            AnsiConsole.WriteLine("Disconnected from server");
        }
        finally
        {
            AnsiConsole.WriteLine("Execution stopped");
        }
    }

    public void Configure()
    {
        AnsiConsole.Write(new Rule("[yellow]Setup[/]").LeftJustified());
        
        _ip = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]ip address[/] of the server")
                .PromptStyle("green")
                .DefaultValue("127.0.0.1")
                .ValidationErrorMessage("[red]That's not a valid ip address[/]")
                .Validate(str => IPAddress.TryParse(str, out _) ? ValidationResult.Success() : ValidationResult.Error()));
        
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

    private async Task<bool> AuthenticateAsync(INetworkClient client, string username, CancellationToken ct)
    {
        var response = await client.SendAsync(new Packet
        {
            Event = "Auth",
            State = new
            {
                Username = username
            }
        }, ct);

        return response is not null && response.Event == "Authenticated";
    }
}