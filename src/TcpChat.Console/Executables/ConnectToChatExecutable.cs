using Spectre.Console;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables;

public class ConnectToChatExecutable : IConfigurableExecutable
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

        var response = await _client.SendAsync(new Packet
        {
            Event = "Connect To Chat",
            State = new
            {
                Id = _chatId
            }
        }, token);

        if (response is null || response.Event != "Connected To Chat")
        {
            AnsiConsole.MarkupLine("An [red]error[/] occurred during connecting to chat");
            return;
        }
        
        await new ChatExecutable(_client, _chatId, _commandParserService).ExecuteAsync(token);
    }

    public void Configure()
    {
        _chatId = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]chat id[/]").PromptStyle("green"));
    }
}
