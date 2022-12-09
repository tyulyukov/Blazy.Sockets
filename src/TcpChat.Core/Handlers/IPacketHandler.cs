using System.Net.Sockets;

namespace TcpChat.Core.Handlers;

public interface IPacketHandler
{
    Task ExecuteAsync(object state, Socket sender, CancellationToken ct = default);
}