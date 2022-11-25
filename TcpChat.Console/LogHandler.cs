using Spectre.Console;
using TcpChat.Core.Interfaces;

namespace TcpChat.Console;

public class LogHandler : ILogHandler
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