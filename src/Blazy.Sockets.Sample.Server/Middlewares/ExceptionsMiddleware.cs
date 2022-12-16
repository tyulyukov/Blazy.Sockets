using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Middlewares;
using Blazy.Sockets.Network;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Middlewares;

public class ExceptionsMiddleware : MiddlewareBase
{
    private readonly ILogger _logger;

    public ExceptionsMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public override async Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next, CancellationToken ct = default)
    {
        try
        {
            await next(request, client, ct);
        }
        catch (Exception exception)
        {
            _logger.Error("Exception of type {ExceptionType} was thrown during handling event {Event} sent by {RemoteEndPoint}", exception.GetType().Name,request.Event, client.RemoteEndPoint);

            try
            {
                await client.SendRequestAsync(new Packet
                {
                    Event = "Error",
                    State = new
                    {
                        exception.Message
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