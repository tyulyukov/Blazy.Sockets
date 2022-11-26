namespace TcpChat.Core.Logging;

public interface ILogHandler
{
    void HandleText(string text);
    void HandleError(Exception exception);
}