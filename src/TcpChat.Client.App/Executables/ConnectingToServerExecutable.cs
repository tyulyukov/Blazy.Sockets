using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class ConnectingToServerExecutable : IExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly IEncoder<Packet> _packetEncoder;
    private readonly IServerCommandParserService _commandParserService;
    
    public ConnectingToServerExecutable(IEncoder<Packet> packetEncoder,
        IServerCommandParserService commandParserService)
    {
        _packetEncoder = packetEncoder;
        _commandParserService = commandParserService;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        AnsiConsole.Write(new Rule("[yellow]Chat[/]").LeftJustified());
        
        /*using var client = new ChatClient(IPAddress.Parse(_ip), _port.Value, _packetEncoder);
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
        }*/
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