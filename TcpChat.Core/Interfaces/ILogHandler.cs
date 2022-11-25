namespace TcpChat.Core.Interfaces;

public interface ILogHandler
{
    void HandleText(string text);
    void HandleError(Exception exception);
}