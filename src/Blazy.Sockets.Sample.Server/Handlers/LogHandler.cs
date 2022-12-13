using Blazy.Sockets.Logging;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class LogHandler : ILogHandler // TODO replace with ILogger
{
    public void HandleText(string text)
    {
        System.Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + text);
    }

    public void HandleError(Exception exception)
    {
        System.Console.WriteLine(exception);
    }
}