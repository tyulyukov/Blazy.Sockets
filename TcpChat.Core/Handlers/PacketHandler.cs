using System.Net.Sockets;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public abstract class PacketHandler : IPacketHandler
{
    private readonly IEncoder<Packet> _packetEncoder;
    protected Socket Sender { get; private set; } = null!;

    protected PacketHandler(IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
    }
    
    public abstract Task HandleAsync(Packet packet, CancellationToken ct);

    public async Task SendResponseAsync(Packet response, CancellationToken ct)
    {
        if (Sender is null)
            throw new ApplicationException("Socket is not scoped");
        
        var request = _packetEncoder.Encode(response);
        _ = await Sender.SendAsync(request, SocketFlags.None, ct);
    }

    public async Task SendErrorAsync(string message, CancellationToken ct)
    {
        if (Sender is null)
            throw new ApplicationException("Socket is not scoped");
        
        var packet = new Packet
        {
            Event = "Error",
            State = new { Message = message }
        };

        var response = _packetEncoder.Encode(packet);
        _ = await Sender.SendAsync(response, SocketFlags.None, ct);
    }

    internal void BeginSocketScope(Socket socket)
    {
        Sender = socket;
    }

    internal void EndSocketScope()
    {
        Sender = null;
    }
}