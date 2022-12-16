using Blazy.Sockets.Network;
using Serilog;
using Serilog.Core;

namespace Blazy.Sockets.Logging;

public static class SerilogExtension
{
    public static NetworkBuilder UseDefaultLogger(this NetworkBuilder builder)
    {
        return builder.Use<ILogger, Logger>(
            new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()
        );
    }
}