using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Serilog;

namespace Blazy.Sockets.Network;
public class SocketAcceptor : ISocketAcceptor
{
    private readonly List<INetworkClient> _clients;
    private readonly object _threadLocker;
    private readonly IPacketHandlersContainer _packetHandlersContainer;
    private readonly ILogger _logger;

    public SocketAcceptor(IPacketHandlersContainer packetHandlersContainer, 
        ILogger logger)
    {
        _clients = new ();
        _packetHandlersContainer = packetHandlersContainer;
        _logger = logger;
        _threadLocker = new ();
    }

    public async Task AcceptSocketAsync(INetworkClient socket, CancellationToken ct = default)
    {
        var connectionDetails = new ConnectionDetails()
        {
            ConnectedAt = DateTime.Now
        };
        
        lock (_threadLocker)
            _clients.Add(socket);

        var handler = _packetHandlersContainer.ResolveConnectionHandler();
        
        if (handler is not null)
        {
            await handler.HandleWithScopedSocketAsync(socket, connectionDetails, ct);
        }
        
        _ = Task.Run(() => ReceivePacketsAsync(socket, connectionDetails, ct));
    }
    
    private async void ReceivePacketsAsync(INetworkClient client, ConnectionDetails connectionDetails,
        CancellationToken ct = default)
    {   
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var request = await client.ReceiveResponseAsync(ct);

                if (request is null)
                    continue;

                // TODO middlewares here

                var handler = _packetHandlersContainer.Resolve(request.Event);
                
                if (handler is null)
                {
                    var message = $"Handler was not found for {request.Event} event";
                    await SendErrorAsync(client, message, ct);
                    continue;
                }
                
                // log "Packet from {client.RemoteEndPoint} handled by {handler.GetType()}"

                // TODO errors middleware
                await handler.ExecuteAsync(request.State, client, ct);
            }
        }
        catch
        {
            var handler = _packetHandlersContainer.ResolveDisconnectionHandler();
            
            if (handler is not null)
            {
                await handler.HandleWithScopedSocketAsync(client, new DisconnectionDetails
                {
                    DisconnectedAt = DateTime.Now,
                    ConnectionTime = DateTime.Now - connectionDetails.ConnectedAt
                }, ct);
            }
        }
        finally
        {
            client.Dispose();
            
            lock (_threadLocker)
                _clients.Remove(client);
        }
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
    
    public void Dispose()
    {
        lock (_threadLocker)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                _clients[i].Disconnect();
                _clients[i].Dispose();
            }
        }
    }
}