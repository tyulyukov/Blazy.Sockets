using Blazy.Sockets.Contracts;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Handlers;

public interface IRequestHandler
{
    Task HandleRequestAsync(Packet request, INetworkClient client, CancellationToken ct = default);
}