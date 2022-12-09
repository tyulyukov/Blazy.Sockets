using System.Net.Sockets;
using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class ConnectToServerExecutable : IExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly INetworkClient _client;
    private readonly ConnectToChatExecutable _connectToChatExe;
    private readonly CreateMyChatExecutable _createMyChatExe;

    public ConnectToServerExecutable(INetworkClient client, 
        ConnectToChatExecutable connectToChatExe, CreateMyChatExecutable createMyChatExe)
    {
        _client = client;
        _connectToChatExe = connectToChatExe;
        _createMyChatExe = createMyChatExe;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        AnsiConsole.Write(new Rule("[yellow]Chat[/]").LeftJustified());
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .StartAsync("Connecting", async ctx =>
            {
                try
                {
                    await _client.ConnectAsync(token);
                    AnsiConsole.MarkupLine("Connected to the server");
                }
                catch (SocketException)
                {
                    AnsiConsole.MarkupLine("An [red]error[/] has occurred while connecting with server");
                }
            });

        if (token.IsCancellationRequested || !_client.Connected)
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
                    if (await AuthenticateAsync(_client, username, token))
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
                _connectToChatExe,
                _createMyChatExe
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