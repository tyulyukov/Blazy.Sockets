using TcpChat.Core.Network;

namespace TcpChat.Core.Handlers;

public interface IPacketHandler
{
    Task ExecuteAsync(object state, INetworkClient sender, CancellationToken ct = default);
}