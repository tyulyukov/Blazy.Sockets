using Blazy.Sockets.Contracts;
using Blazy.Sockets.Network;
using Blazy.Sockets.Sample.Client.Domain;
using Blazy.Sockets.Sample.Client.Models;
using Spectre.Console;

namespace Blazy.Sockets.Sample.Client.Executables;

public class CreateMyChatExecutable : IExecutable
{
    public string RepresentationText => "Create my chat";
    
    private readonly INetworkClient _client;
    private readonly ChatExecutable _chatExe;

    public CreateMyChatExecutable(INetworkClient client, ChatExecutable chatExe)
    {
        _client = client;
        _chatExe = chatExe;
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

        chat.Id = response.State.ToString()!;
        
        _chatExe.Initialize(chat);
        await _chatExe.ExecuteAsync(token);
    }
}
