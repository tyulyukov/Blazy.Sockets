using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Middlewares;

public interface IMiddleware
{
    void SetNext(PacketDelegate next);
    Task InvokeAsync(Packet request, INetworkClient client, CancellationToken ct = default);
}