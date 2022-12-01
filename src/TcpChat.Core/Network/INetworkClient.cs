using TcpChat.Core.Contracts;

namespace TcpChat.Core.Network;

public interface INetworkClient : IDisposable
{
    bool Connected { get; }
    
    Task ConnectAsync(CancellationToken ct);
    void Disconnect();
    Task SendRequestAsync(Packet packet, CancellationToken ct);
    Task<Packet?> ReceiveResponseAsync(CancellationToken ct);
    Task<Packet?> SendAsync(Packet packet, CancellationToken ct);
}