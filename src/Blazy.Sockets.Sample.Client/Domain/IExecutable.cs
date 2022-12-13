namespace Blazy.Sockets.Sample.Client.Domain;

public interface IExecutable
{
    string RepresentationText { get; }
    Task ExecuteAsync(CancellationToken token);
}