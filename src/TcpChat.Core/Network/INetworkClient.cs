using TcpChat.Core.Contracts;

namespace TcpChat.Core.Network;

public interface INetworkClient : IDisposable
{
    bool Connected { get; }
    
    Task ConnectAsync(CancellationToken ct = default);
    void Disconnect();
    
    Task<Packet?> SendWithTimeOutAsync(Packet packet, TimeSpan timeout, CancellationToken ct = default);
    Task<Packet?> SendAsync(Packet packet, CancellationToken ct = default);
    
    Task SendRequestAsync(Packet packet, CancellationToken ct = default);
    
    Task<Packet?> ReceiveResponseWithTimeOutAsync(TimeSpan timeout, CancellationToken ct = default);
    Task<Packet?> ReceiveResponseAsync(CancellationToken ct = default);
}