using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Middlewares;

public class MiddlewaresChain : IMiddlewaresChain
{
    private readonly PacketDelegate _packetDelegate;
    
    public MiddlewaresChain(IEnumerable<IMiddleware> middlewares, IRequestHandler requestHandler)
    {
        IMiddleware? headMiddleware = null;
        IMiddleware? prevMiddleware = null;
        
        foreach (var middleware in middlewares)
        {
            headMiddleware ??= middleware;
            prevMiddleware?.SetNext(middleware.InvokeAsync);
            prevMiddleware = middleware;
        }
        
        if (headMiddleware is null)
        {
            _packetDelegate = requestHandler.HandleRequestAsync;
        }
        else
        {
            prevMiddleware!.SetNext(requestHandler.HandleRequestAsync);
            _packetDelegate = headMiddleware.InvokeAsync;
        }
    }

    public async Task InvokeAsync(Packet request, INetworkClient sender, CancellationToken ct = default)
    {
        await _packetDelegate(request, sender, ct);
    }
}