using Spectre.Console;
using TcpChat.Client.App.Domain;
using TcpChat.Client.App.Models;
using TcpChat.Client.App.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Network;

namespace TcpChat.Client.App.Executables;

public class ChatExecutable : IExecutable
{
    public string RepresentationText => "Send and receive messages in chat";

    private readonly INetworkClient _client;
    private readonly IServerCommandParserService _commandParserService;

    private string? _chatId;
    private Chat? _chat;
    
    public ChatExecutable(INetworkClient client, IServerCommandParserService commandParserService)
    {
        _client = client;
        _commandParserService = commandParserService;
    }

    public void Initialize(string chatId, Chat chat)
    {
        _chatId = chatId;
        _chat = chat;
    }
    
    public Task ExecuteAsync(CancellationToken token)
    {
        if (_chatId is null || _chat is null)
            throw new ApplicationException("Executable is not initialized");
        
        AnsiConsole.Write(new Rule($"[yellow]{_chat.Name}[/]").LeftJustified());
        AnsiConsole.MarkupLine("[grey]Available commands:[/]");
        // TODO print available commands from command parser
        AnsiConsole.MarkupLine("[grey]/leave[/]");

        using var scopedCts = new CancellationTokenSource();

        var receiveMessagesTask = ReceiveMessagesAsync(scopedCts.Token, () => scopedCts.Cancel());
        var sendMessagesTask = SendMessagesAsync(scopedCts.Token);

        Task.WaitAll(new[] { receiveMessagesTask, sendMessagesTask }, token);
        return Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync(CancellationToken ct, Action leaveChat)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // TODO implement asynchronous receiver with handlers in Core
                var packet = await _client.ReceiveResponseAsync(ct);

                if (packet is null) 
                    continue;
                
                if (packet.Event == "Left Chat")
                {
                    leaveChat();
                    break;
                }
                
                AnsiConsole.WriteLine($"Incoming packet {packet.State}");
                
            }
        }
        catch (SocketDisconnectedException) { }
    }

    private async Task SendMessagesAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var message = AnsiConsole.Prompt(
                    new TextPrompt<string>(">")
                        .PromptStyle("yellow")
                        .Validate(str => str.Length <= 128)
                        .ValidationErrorMessage("nah bro its [red]too long[/]"));

                // this will be in parser or smth similar
                if (message.Trim() == "/leave")
                {
                    await _client.SendRequestAsync(new Packet
                    {
                        Event = "Leave Chat",
                        State = new
                        {
                            Id = _chatId,
                        }
                    }, ct);
                    break;
                }

                if (_commandParserService.TryParse(message, out _))
                {
                    // Do smth with the command
                    continue;
                }

                await _client.SendRequestAsync(new Packet
                {
                    Event = "Message",
                    State = new
                    {
                        Chat = _chatId,
                        Message = message
                    }
                }, ct);
            }
        }
        catch (SocketDisconnectedException) { }
    }
}