using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public interface IHandlersCollection
{
    string ConnectedEventName { get; }
    string DisconnectedEventName { get; }
    
    void Register(string eventName, IPacketHandler handler);
    void RegisterConnectionHandler(PacketHandler<ConnectionDetails> handler);
    void RegisterDisconnectionHandler(PacketHandler<DisconnectionDetails> handler);
    
    IPacketHandler? Resolve(string eventName);
    PacketHandler<ConnectionDetails>? ResolveConnectionHandler();
    PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler();
}