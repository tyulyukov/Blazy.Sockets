using System.Text.Json;
using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Client.App.Models;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class ConnectToChatExecutable : IExecutable
{
    public string RepresentationText => "Connect to chat";
    
    private readonly INetworkClient _client;
    private readonly ChatExecutable _chatExe;
    
    public ConnectToChatExecutable(INetworkClient client, ChatExecutable chatExe)
    {
        _client = client;
        _chatExe = chatExe;
    }
    
    public async Task ExecuteAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        var chatId = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]chat id[/]").PromptStyle("green"));
        
        var response = await _client.SendAsync(new Packet
        {
            Event = "Connect To Chat",
            State = new
            {
                Id = chatId
            }
        }, token);

        if (response is null)
        {
            AnsiConsole.MarkupLine("An [red]error[/] occurred during connecting to chat");
            return;
        }

        if (response.Event != "Connected To Chat")
        {
            AnsiConsole.MarkupLine($"Chat with id [yellow]{chatId}[/] [red]does not exist[/]");
            return;
        }

        var chat = JsonDocument.Parse(response.State.ToString() ?? string.Empty).Deserialize<Chat>();

        if (chat is null)
        {
            AnsiConsole.MarkupLine($"An [red]error[/] occurred during chat deserializing");
            return;
        }
        
        _chatExe.Initialize(chatId, chat);
        await _chatExe.ExecuteAsync(token);
    }
}
