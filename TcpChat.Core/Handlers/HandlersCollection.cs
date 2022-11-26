namespace TcpChat.Core.Handlers;

public class HandlersCollection : IHandlersCollection
{
    private readonly Dictionary<string, PacketHandler> _handlers;

    public HandlersCollection()
    {
        _handlers = new();
    }

    public void Register(string eventName, PacketHandler handler)
    {
        if (_handlers.ContainsKey(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");
        
        _handlers.Add(eventName, handler);
    }
    
    public PacketHandler? Resolve(string eventName)
    {
        _handlers.TryGetValue(eventName, out var handler);
        return handler;
    }
}