namespace Blazy.Sockets.Network;

public interface INetworkServer : IDisposable
{
    Task RunAsync(CancellationToken ct = default);
}