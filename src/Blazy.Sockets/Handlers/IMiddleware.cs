using Blazy.Sockets.Contracts;

namespace Blazy.Sockets.Handlers;

public interface IMiddleware
{
    Task InvokeAsync(ref Packet incomingPacket, PacketDelegate? next);
}