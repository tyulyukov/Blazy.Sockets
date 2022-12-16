using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Middlewares;

public abstract class MiddlewareBase : IMiddleware
{
    private PacketDelegate? _next;

    public void SetNext(PacketDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(Packet request, INetworkClient client, CancellationToken ct)
    {
        if (_next is null)
            throw new ApplicationException("Next Packet Delegate is null");
        
        await InvokeAsync(request, client, _next, ct);
    }

    public abstract Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next,
        CancellationToken ct = default);
}