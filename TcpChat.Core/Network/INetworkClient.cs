using TcpChat.Core.Contracts;

namespace TcpChat.Core.Network;

public interface INetworkClient : IDisposable
{
    Task ConnectAsync(CancellationToken ct);
    void Disconnect();
    Task<Packet?> SendAsync(Packet packet, CancellationToken ct);
}

public interface INetworkServer : IDisposable
{
    Task RunAsync(CancellationToken ct);
}