using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public interface IMiddleware
{
    Task InvokeAsync(ref Packet incomingPacket, PacketDelegate? next);
}