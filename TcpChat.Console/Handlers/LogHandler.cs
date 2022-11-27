using Spectre.Console;
using TcpChat.Core.Logging;

namespace TcpChat.Console.Handlers;

public class LogHandler : ILogHandler // TODO replace with ILogger
{
    public void HandleText(string text)
    {
        AnsiConsole.MarkupLine(text);
    }

    public void HandleError(Exception exception)
    {
        AnsiConsole.WriteException(exception);
    }
}