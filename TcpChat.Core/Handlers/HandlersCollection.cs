namespace TcpChat.Core.Handlers;

public class HandlersCollection : IHandlersCollection
{
    private readonly Dictionary<string, IPacketHandler> _handlers;

    public HandlersCollection()
    {
        _handlers = new();
    }

    public void Register(string eventName, IPacketHandler handler)
    {
        if (_handlers.ContainsKey(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");
        
        _handlers.Add(eventName, handler);
    }
    
    public IPacketHandler? Resolve(string eventName)
    {
        _handlers.TryGetValue(eventName, out var handler);
        return handler;
    }
}