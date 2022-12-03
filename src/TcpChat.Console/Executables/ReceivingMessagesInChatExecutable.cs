namespace TcpChat.Console.Executables;

public class ReceivingMessagesInChatExecutable : IExecutable
{
    public string RepresentationText => "Receive messages in chat";
    
    public Task ExecuteAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}