using Blazy.Sockets.Network;

namespace Blazy.Sockets.Handlers;

public interface IPacketHandler
{
    Task ExecuteAsync(object state, INetworkClient sender, CancellationToken ct = default);
}