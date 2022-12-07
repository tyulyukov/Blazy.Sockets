namespace TcpChat.Client.App.Domain;

public interface IExecutable
{
    string RepresentationText { get; }
    Task ExecuteAsync(CancellationToken token);
}