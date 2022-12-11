using Autofac;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Core.Network;

public class PacketHandlersContainer : IPacketHandlersContainer
{
    public const string ConnectedEventName = "Connection";
    public const string DisconnectedEventName = "Disconnection";
    
    private readonly ILifetimeScope _scope;

    public PacketHandlersContainer(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public IPacketHandler? Resolve(string eventName)
    {
        return _scope.ResolveOptionalNamed<IPacketHandler>(eventName);
    }

    public PacketHandler<ConnectionDetails>? ResolveConnectionHandler()
    {
        return _scope.ResolveOptionalNamed<PacketHandler<ConnectionDetails>>(ConnectedEventName);
    }

    public PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler()
    {
        return _scope.ResolveOptionalNamed<PacketHandler<DisconnectionDetails>>(DisconnectedEventName);
    }
}