using TcpChat.Core.Contracts;

namespace TcpChat.Core.Network;

public interface INetworkClient : IDisposable
{
    bool Connected { get; }
    
    Task ConnectAsync(CancellationToken ct);
    void Disconnect();
    
    Task<Packet?> SendWithTimeOutAsync(Packet packet, TimeSpan timeout, CancellationToken ct);
    Task<Packet?> SendAsync(Packet packet, CancellationToken ct);
    
    Task SendRequestAsync(Packet packet, CancellationToken ct);
    
    Task<Packet?> ReceiveResponseWithTimeOutAsync(TimeSpan timeout, CancellationToken ct);
    Task<Packet?> ReceiveResponseAsync(CancellationToken ct);
}