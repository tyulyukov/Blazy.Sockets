namespace Blazy.Sockets.Logging;

public interface ILogHandler
{
    void HandleText(string message);
    void HandleError(Exception exception);
}