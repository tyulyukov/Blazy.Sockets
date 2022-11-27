using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

internal interface IPacketHandler
{
    Task HandleAsync(Packet packet, CancellationToken ct);
}