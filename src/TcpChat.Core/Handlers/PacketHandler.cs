using System.Text.Json;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

namespace TcpChat.Core.Handlers;

public abstract class PacketHandler<TRequest> : IPacketHandler where TRequest : notnull, new()
{
    private readonly IEncoder<Packet> _packetEncoder;
    protected INetworkClient Sender { get; private set; } = null!;

    protected PacketHandler(IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(object state, INetworkClient sender, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(state);
        var request = JsonDocument.Parse(json).Deserialize<TRequest>();
        
        if (request is null)
        {
            await SendErrorAsync("Bad request", ct);
            return;
        }
        
        await HandleWithScopedSocketAsync(sender, request, ct);
    }

    public abstract Task HandleAsync(TRequest request, CancellationToken ct = default);
    
    protected async Task SendResponseAsync(Packet response, CancellationToken ct = default)
    {
        await SendResponseAsync(Sender, response, ct);
    }
    
    protected async Task SendResponseAsync(INetworkClient client, Packet response, CancellationToken ct = default)
    {
        // here it is a little confusing, its alright we are sending response
        await client.SendRequestAsync(response, ct);
    }

    protected async Task SendErrorAsync(string message, CancellationToken ct = default)
    {
        if (Sender is null)
            throw new ApplicationException("Network Client is not scoped");
        
        var packet = new Packet
        {
            Event = "Error",
            State = new { Message = message }
        };

        await Sender.SendRequestAsync(packet, ct);
    }

    public async Task HandleWithScopedSocketAsync(INetworkClient sender, TRequest request, CancellationToken ct = default)
    {
        Sender = sender;
        await HandleAsync(request, ct);
        Sender = null!;
    }
}