using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;

namespace Blazy.Sockets.Network;

public interface IPacketHandlersContainer
{
    IPacketHandler? Resolve(string eventName);
    PacketHandler<ConnectionDetails>? ResolveConnectionHandler();
    PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler();
}