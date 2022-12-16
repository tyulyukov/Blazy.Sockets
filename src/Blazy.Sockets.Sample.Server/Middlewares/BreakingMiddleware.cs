using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Middlewares;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Sample.Server.Middlewares;

// use this middleware to test your application if handling fails
public class BreakingMiddleware : MiddlewareBase
{
    public override async Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next, CancellationToken ct = default)
    {
        request.Event = "unknown";
        await next(request, client, ct);
    }
}