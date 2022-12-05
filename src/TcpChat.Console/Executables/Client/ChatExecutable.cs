using Spectre.Console;
using TcpChat.Console.Domain;
using TcpChat.Console.Models;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables.Client;

public class ChatExecutable : IExecutable
{
    public string RepresentationText => "Send and receive messages in chat";

    private readonly INetworkClient _client;
    private readonly string _chatId;
    private readonly Chat _chat;
    private readonly IServerCommandParserService _commandParserService;

    public ChatExecutable(INetworkClient client, string chatId, Chat chat, IServerCommandParserService commandParserService)
    {
        _client = client;
        _chatId = chatId;
        _chat = chat;
        _commandParserService = commandParserService;
    }
    
    public Task ExecuteAsync(CancellationToken token)
    {
        AnsiConsole.Write(new Rule($"[yellow]{_chat.Name}[/]").LeftJustified());
        AnsiConsole.MarkupLine("[grey]Type message[/]");
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