using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public delegate Task PacketDelegate(Packet incomingPacket, PacketDelegate? next);