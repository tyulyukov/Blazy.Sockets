namespace TcpChat.Console.Domain;

public interface IExecutable
{
    string RepresentationText { get; }
    Task ExecuteAsync(CancellationToken token);
}