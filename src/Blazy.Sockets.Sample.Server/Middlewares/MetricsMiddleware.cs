using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Middlewares;
using Blazy.Sockets.Network;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Middlewares;

public class MetricsMiddleware : MiddlewareBase
{
    private readonly ILogger _logger;

    public MetricsMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public override async Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next, CancellationToken ct = default)
    {
        var startDate = DateTime.UtcNow;
        
        await next(request, client, ct);
        
        var endDate = DateTime.UtcNow;
        var elapsed = (endDate - startDate).TotalMilliseconds;
        
        _logger.Information("Packet for {Event} sent by {RemoteEndPoint} was handled in {ElapsedMilliseconds}ms", request.Event, client.RemoteEndPoint, elapsed);
    }
}