using System.Net.Sockets;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Socket sender, CancellationToken ct);
}