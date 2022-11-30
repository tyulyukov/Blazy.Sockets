namespace TcpChat.Console.Executables;

public interface IExecutable
{
    string RepresentationText { get; }
    Task ExecuteAsync(CancellationToken token);
}