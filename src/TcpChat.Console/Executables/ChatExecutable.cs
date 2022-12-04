using Spectre.Console;
using TcpChat.Console.Services;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Network;

namespace TcpChat.Console.Executables;

public class ChatExecutable : IExecutable
{
    public string RepresentationText => "Send and receive messages in chat";

    private readonly INetworkClient _client;
    private readonly string _chatId;
    private readonly IServerCommandParserService _commandParserService;

    public ChatExecutable(INetworkClient client, string chatId, IServerCommandParserService commandParserService)
    {
        _client = client;
        _chatId = chatId;
        _commandParserService = commandParserService;
    }
    
    public Task ExecuteAsync(CancellationToken token)
    {
        using var scopedCts = new CancellationTokenSource();

        var receiveMessagesTask = ReceiveMessagesAsync(scopedCts.Token);
        var sendMessagesTask = SendMessagesAsync(scopedCts);

        Task.WaitAll(new[] { receiveMessagesTask, sendMessagesTask }, token);
        return Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // TODO implement asynchronous receiver with handlers in Core
                var packet = await _client.ReceiveResponseAsync(ct);

                if (packet is not null)
                {
                    AnsiConsole.WriteLine($"Incoming packet {packet.State}");
                }
            }
        }
        catch (SocketDisconnectedException) { }
    }

    private async Task SendMessagesAsync(CancellationTokenSource cts)
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                var message = AnsiConsole.Prompt(
                    new TextPrompt<string>(">")
                        .PromptStyle("yellow")
                        .Validate(str => str.Length <= 128)
                        .ValidationErrorMessage("nah bro its [red]too long[/]"));

                // this will be in parser or smth similar
                if (message.Trim() == "/leave")
                {
                    cts.Cancel();
                    return;
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
                }, cts.Token);
            }
        }
        catch (SocketDisconnectedException) { }
    }
}