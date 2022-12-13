namespace Blazy.Sockets.Network;

public interface ISocketAcceptor : IDisposable
{
    Task AcceptSocketAsync(INetworkClient socket, CancellationToken ct = default);
}