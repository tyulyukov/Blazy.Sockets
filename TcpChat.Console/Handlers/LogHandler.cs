using Spectre.Console;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class LogHandler : ILogHandler // TODO replace with ILogger
{
    public void HandleText(string text)
    {
        AnsiConsole.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + text);
    }

    public void HandleError(Exception exception)
    {
        AnsiConsole.WriteException(exception);
    }
}