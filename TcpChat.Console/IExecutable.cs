namespace TcpChat.Console;

public interface IExecutable
{
    string RepresentationText { get; }
    Task ExecuteAsync(CancellationToken token);
    void Configure();
}