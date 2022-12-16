using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Middlewares;

namespace Blazy.Sockets.Network;
public class SocketAcceptor : ISocketAcceptor
{
    private readonly List<INetworkClient> _clients;
    private readonly object _threadLocker;
    private readonly IPacketHandlersContainer _packetHandlersContainer;
    private readonly IMiddlewaresChain _middlewares;

    public SocketAcceptor(IPacketHandlersContainer packetHandlersContainer, IMiddlewaresChain middlewares)
    {
        _clients = new ();
        _packetHandlersContainer = packetHandlersContainer;
        _middlewares = middlewares;
        _threadLocker = new ();
    }

    public async Task AcceptSocketAsync(INetworkClient socket, CancellationToken ct = default)
    {
        var connectionDetails = new ConnectionDetails
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

                await _middlewares.InvokeAsync(request, client, ct);
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