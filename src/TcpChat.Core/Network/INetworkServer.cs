namespace TcpChat.Core.Network;

public interface INetworkServer : IDisposable
{
    Task RunAsync(CancellationToken ct);
}