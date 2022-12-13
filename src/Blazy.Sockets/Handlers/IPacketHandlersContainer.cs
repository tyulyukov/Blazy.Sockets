using Blazy.Sockets.Contracts;

namespace Blazy.Sockets.Handlers;

public interface IPacketHandlersContainer
{
    IPacketHandler? Resolve(string eventName);
    PacketHandler<ConnectionDetails>? ResolveConnectionHandler();
    PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler();
}