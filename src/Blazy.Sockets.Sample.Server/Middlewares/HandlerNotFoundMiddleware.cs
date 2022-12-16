using Blazy.Sockets.Contracts;
using Blazy.Sockets.Exceptions;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Middlewares;
using Blazy.Sockets.Network;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Middlewares;

public class HandlerNotFoundMiddleware : MiddlewareBase
{
    private readonly ILogger _logger;

    public HandlerNotFoundMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public override async Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next, CancellationToken ct = default)
    {
        try
        {
            await next(request, client, ct);
        }
        catch (HandlerWasNotFound exception)
        {
            _logger.Information("Handler for event {Event} sent by {RemoteEndPoint} was not found",request.Event, client.RemoteEndPoint);

            try
            {
                await client.SendRequestAsync(new Packet
                {
                    Event = "Error",
                    State = new
                    {
                        Message = $"Handler was not found for event {request.Event}"
                    }
                }, ct);
            }
            catch
            {
                // ignored
            }
        }
    }
}