using System.Net.Sockets;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, TcpClient sender, CancellationToken ct);
}