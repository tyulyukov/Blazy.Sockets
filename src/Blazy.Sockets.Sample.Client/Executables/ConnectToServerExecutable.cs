﻿using Blazy.Sockets.Contracts;
using Blazy.Sockets.Exceptions;
using Blazy.Sockets.Network;
using Blazy.Sockets.Sample.Client.Domain;
using Blazy.Sockets.Sample.Client.Models;
using Blazy.Sockets.Sample.Client.Services;
using Spectre.Console;

namespace Blazy.Sockets.Sample.Client.Executables;

public class ConnectToServerExecutable : IExecutable
{
    public string RepresentationText => "Connect to the server";

    private readonly INetworkClient _client;
    private readonly IUserStorage _userStorage;
    private readonly ConnectToChatExecutable _connectToChatExe;
    private readonly CreateMyChatExecutable _createMyChatExe;

    public ConnectToServerExecutable(INetworkClient client, IUserStorage userStorage,
        ConnectToChatExecutable connectToChatExe, CreateMyChatExecutable createMyChatExe)
    {
        _client = client;
        _userStorage = userStorage;
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
                catch
                {
                    AnsiConsole.MarkupLine("An [red]error[/] has occurred while connecting with server");
                    throw;
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
                if (await AuthenticateAsync(_client, username, token))
                    break;

                username = AnsiConsole.Prompt(
                    new TextPrompt<string>(
                            $"Username {username} is [red]already taken[/]. Try [green]another one[/]")
                        .PromptStyle("green"));
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
        catch (SocketDisconnectedException)
        {
            AnsiConsole.WriteLine("Disconnected from server");
        }
        finally
        {
            _userStorage.LogOut();
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

        if (response is null)
            throw new ApplicationException("Response was not received");

        if (response.Event == "Username Is Taken")
            return false;

        if (response.Event != "Authenticated")
            throw new ApplicationException("Request was handled incorrectly");
        
        _userStorage.Authenticate(new User { Name = username });

        return true;
    }
}