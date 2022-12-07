using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class CreateMyChatExecutable : IExecutable
{
    public string RepresentationText => "Create my chat";
    
    private readonly INetworkClient _client;
    private readonly IServerCommandParserService _commandParserService;

    public CreateMyChatExecutable(INetworkClient client, IServerCommandParserService commandParserService)
    {
        _client = client;
        _commandParserService = commandParserService;
    }
    
    public async Task ExecuteAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        
        var chatName = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]chat name[/]").PromptStyle("green"));

        Packet? response = null;
        
        var chat = new Chat
        {
            Name = chatName
        };
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.SimpleDotsScrolling)
            .StartAsync("Creating chat", async ctx =>
            {
                await _client.SendRequestAsync(new Packet
                {
                    Event = "Create Chat", 
                    State = chat
                }, token);

                ctx.Status("Receiving response");

                response = await _client.ReceiveResponseAsync(token);
            });

        if (response is null || response.Event != "Chat Created" || response.State.ToString() is null)
        {
            AnsiConsole.MarkupLine("An [red]error[/] has occurred while creating chat");
            return;
        }

        AnsiConsole.MarkupLine($"Chat {chatName} created with id [yellow]{response.State}[/]");
        AnsiConsole.MarkupLine("[grey]Share this id to chat with someone[/]");

        await new ChatExecutable(_client, response.State.ToString()!, chat, _commandParserService).ExecuteAsync(token);
    }
}
