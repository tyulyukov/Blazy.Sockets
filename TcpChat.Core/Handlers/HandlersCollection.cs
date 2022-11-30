using System.Collections.Concurrent;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public class HandlersCollection : IHandlersCollection
{
    public string ConnectedEventName => "Connection";
    public string DisconnectedEventName => "Disconnection";

    private readonly ConcurrentDictionary<string, IPacketHandler> _handlers;

    public HandlersCollection()
    {
        _handlers = new();
    }

    public void Register(string eventName, IPacketHandler handler)
    {
        if (_handlers.ContainsKey(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");
        
        _handlers.TryAdd(eventName, handler);
    }

    public void RegisterConnectionHandler(PacketHandler<ConnectionDetails> handler)
    {
        if (_handlers.ContainsKey(ConnectedEventName))
            throw new ApplicationException("Connection handler already exists");
        
        _handlers.TryAdd(ConnectedEventName, handler);
    }

    public void RegisterDisconnectionHandler(PacketHandler<DisconnectionDetails> handler)
    {
        if (_handlers.ContainsKey(DisconnectedEventName))
            throw new ApplicationException("Disconnection handler already exists");
        
        _handlers.TryAdd(DisconnectedEventName, handler);
    }

    
    public PacketHandler<ConnectionDetails>? ResolveConnectionHandler()
    {
        _handlers.TryGetValue(ConnectedEventName, out var handler);
        return handler as PacketHandler<ConnectionDetails>;
    }
    
    public PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler()
    {
        _handlers.TryGetValue(DisconnectedEventName, out var handler);
        return handler as PacketHandler<DisconnectionDetails>;
    }

    public IPacketHandler? Resolve(string eventName)
    {
        _handlers.TryGetValue(eventName, out var handler);
        return handler;
    }
}