namespace TcpChat.Core.Network;

public interface ISocketAcceptor : IDisposable
{
    Task AcceptSocketAsync(INetworkClient socket, CancellationToken ct = default);
}