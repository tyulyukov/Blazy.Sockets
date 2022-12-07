using System.Net.Sockets;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public interface IPacketHandler
{
    Task ExecuteAsync(object state, Socket sender, CancellationToken ct = default);
}