using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Core.Network;

public interface IPacketHandlersContainer
{
    IPacketHandler? Resolve(string eventName);
    PacketHandler<ConnectionDetails>? ResolveConnectionHandler();
    PacketHandler<DisconnectionDetails>? ResolveDisconnectionHandler();
}