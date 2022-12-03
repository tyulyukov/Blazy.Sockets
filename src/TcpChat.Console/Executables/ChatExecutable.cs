namespace TcpChat.Console.Executables;

public class ChatExecutable : IExecutable
{
    public string RepresentationText => "Send and receive messages in chat";

    private readonly string _chatId;
    
    public ChatExecutable(string chatId)
    {
        _chatId = chatId;
    }
    
    public Task ExecuteAsync(CancellationToken token)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        
        
        
        cts.Cancel();
        return Task.CompletedTask;
    }

    private Task ReceiveMessagesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            
        }

        return Task.CompletedTask;
    }

    private Task SendMessagesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            
        }

        return Task.CompletedTask;
    }
}