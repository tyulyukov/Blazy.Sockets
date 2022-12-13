using Blazy.Sockets.Contracts;

namespace Blazy.Sockets.Handlers;

public delegate Task PacketDelegate(Packet incomingPacket, PacketDelegate? next);