using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Middlewares;

public interface IMiddleware
{
    Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate? next, CancellationToken ct = default);
}