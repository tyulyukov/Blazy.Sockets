using Spectre.Console;
using TcpChat.Console.Models;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables;

public class CreateMyChatExecutable : IExecutable
{
    public string RepresentationText => "Create my chat";
    private readonly INetworkClient _client;
    
    public CreateMyChatExecutable(INetworkClient client)
    {
        _client = client;
    }
    
    public async Task ExecuteAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested || !_client.Connected)
            return;
        
        var chatName = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]chat name[/]").PromptStyle("green"));

        Packet? response = null;
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.SimpleDotsScrolling)
            .StartAsync("Creating chat", async ctx =>
            {
                await _client.SendRequestAsync(new Packet
                {
                    Event = "Create Chat", 
                    State = new Chat
                    {
                        Name = chatName,
                        Creator = new User()
                        {
                            Name = "user123"
                        },
                        Users = new List<User>()
                    }
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

        await new ChatExecutable(response.State.ToString()!).ExecuteAsync(token);
    }
}
