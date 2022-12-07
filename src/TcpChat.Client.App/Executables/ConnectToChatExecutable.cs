using System.Text.Json;
using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class ConnectToChatExecutable : IExecutable
{
    public string RepresentationText => "Connect to chat";
    
    private readonly INetworkClient _client;
    private readonly IServerCommandParserService _commandParserService;

    private string? _chatId;
    
    public ConnectToChatExecutable(INetworkClient client, IServerCommandParserService commandParserService)
    {
        _client = client;
        _commandParserService = commandParserService;
    }
    
    public async Task ExecuteAsync(CancellationToken token)
    {
        if (_chatId is null)
            throw new ApplicationException("Executable is not configured");
        
        if (token.IsCancellationRequested)
            return;

        var response = await _client.SendAsync(new Packet
        {
            Event = "Connect To Chat",
            State = new
            {
                Id = _chatId
            }
        }, token);

        if (response is null)
        {
            AnsiConsole.MarkupLine("An [red]error[/] occurred during connecting to chat");
            return;
        }

        if (response.Event != "Connected To Chat")
        {
            AnsiConsole.MarkupLine($"Chat with id [yellow]{_chatId}[/] [red]does not exist[/]");
            return;
        }

        var chat = JsonDocument.Parse(response.State.ToString() ?? string.Empty).Deserialize<Chat>();

        if (chat is null)
        {
            AnsiConsole.MarkupLine($"An [red]error[/] occurred during chat deserializing");
            return;
        }
        
        await new ChatExecutable(_client, _chatId, chat, _commandParserService).ExecuteAsync(token);
    }
}
