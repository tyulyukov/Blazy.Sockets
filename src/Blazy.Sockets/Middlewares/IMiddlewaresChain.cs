using Blazy.Sockets.Contracts;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Middlewares;

public interface IMiddlewaresChain
{
    Task InvokeAsync(Packet request, INetworkClient sender, CancellationToken ct = default);
}