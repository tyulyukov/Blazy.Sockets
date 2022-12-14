using Autofac;
using Blazy.Sockets.Contracts;

namespace Blazy.Sockets.Handlers;

public class PacketHandlersContainer : IPacketHandlersContainer
{
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
        return _scope.ResolveOptional<PacketHandler<ConnectionDetails>>();
    }

    public PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler()
    {
        return _scope.ResolveOptional<PacketHandler<DisconnectionDetails>>();
    }
}