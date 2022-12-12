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
    private readonly IPacketHandlersContainer _packetHandlersContainer;
    private readonly IUserStorage _userStorage;

    private Chat? _chat;

    private int _consoleRowsPassed;
    
    public ChatExecutable(INetworkClient client, IServerCommandParserService commandParserService, 
        IPacketHandlersContainer packetHandlersContainer, IUserStorage userStorage)
    {
        _client = client;
        _commandParserService = commandParserService;
        _packetHandlersContainer = packetHandlersContainer;
        _userStorage = userStorage;
    }

    public void Initialize(Chat chat)
    {
        _chat = chat;
    }
    
    public Task ExecuteAsync(CancellationToken token)
    {
        if (_chat is null)
            throw new ApplicationException("Executable is not initialized");
        
        _userStorage.JoinChat(_chat);
        
        AnsiConsole.Write(new Rule($"[yellow]{_chat.Name}[/]").LeftJustified());
        AnsiConsole.MarkupLine("[grey]Available commands:[/]");
        // TODO print available commands from command parser
        AnsiConsole.MarkupLine("[grey]/leave[/]");

        using var scopedCts = new CancellationTokenSource();

        var receiveMessagesTask = ReceiveMessagesAsync(scopedCts.Token, () => scopedCts.Cancel());
        var sendMessagesTask = SendMessagesAsync(scopedCts.Token);

        Task.WaitAll(new[] { receiveMessagesTask, sendMessagesTask }, token);
        
        _userStorage.LeaveChat();
        
        return Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync(CancellationToken ct, Action leaveChat)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var packet = await _client.ReceiveResponseAsync(ct);

                if (packet is null) 
                    continue;
                
                if (packet.Event is "Left Chat" or "Kicked From Chat")
                {
                    leaveChat();
                    break;
                }

                var handler = _packetHandlersContainer.Resolve(packet.Event);

                if (handler is null) 
                    continue;
                
                var prevLeft = Console.CursorLeft;
                var prevTop = Console.CursorTop;

                _consoleRowsPassed++;
                Console.CursorTop += _consoleRowsPassed;
                Console.CursorLeft = 0;
                    
                await handler.ExecuteAsync(packet.State, _client, ct);
                    
                Console.CursorTop = prevTop;
                Console.CursorLeft = prevLeft;
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
                _consoleRowsPassed = 0;
                var message = AnsiConsole.Prompt(new TextPrompt<string>(">")
                    .PromptStyle("yellow")
                    .Validate(str => str.Length <= 128)
                    .ValidationErrorMessage("nah bro its [red]too long[/]"));

                Console.CursorTop += _consoleRowsPassed;

                // this will be in parser or smth similar
                if (message.Trim() == "/leave")
                {
                    await _client.SendRequestAsync(new Packet
                    {
                        Event = "Leave Chat",
                        State = new
                        {
                            _chat?.Id,
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
                        Chat = _chat?.Id,
                        Message = message
                    }
                }, ct);
                
                // AnsiConsole.MarkupLine($"[yellow]<me> {message}[/]");
            }
        }
        catch (SocketDisconnectedException) { }
    }
}