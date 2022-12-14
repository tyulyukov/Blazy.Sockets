using Blazy.Sockets.Contracts;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Handlers;

public class RequestHandler : IRequestHandler
{
    private readonly IPacketHandlersContainer _packetHandlersContainer;

    public RequestHandler(IPacketHandlersContainer packetHandlersContainer)
    {
        _packetHandlersContainer = packetHandlersContainer;
    }
    
    public async Task HandleRequestAsync(Packet request, INetworkClient client, CancellationToken ct = default)
    {
        // TODO middlewares here

        var handler = _packetHandlersContainer.Resolve(request.Event);
                
        if (handler is null)
        {
            var message = $"Handler was not found for {request.Event} event";
            await SendErrorAsync(client, message, ct);
            return;
        }
                
        // log "Packet from {client.RemoteEndPoint} handled by {handler.GetType()}"

        await handler.ExecuteAsync(request.State, client, ct);
    }
    
    private async Task SendErrorAsync(INetworkClient client, string message, CancellationToken ct = default)
    {         
        var packet = new Packet
        {
            Event = "Error",
            State = new { Message = message }
        };

        _ = await client.SendAsync(packet, ct);
    }
}