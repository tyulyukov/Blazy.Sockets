using Blazy.Sockets.Network;
using Serilog;
using Serilog.Core;

namespace Blazy.Sockets.Logging;

public static class SerilogExtension
{
    public static void UseDefaultLogger(this NetworkBuilder builder)
    {
        builder.Use<ILogger, Logger>(
            new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()
        );
    }
}