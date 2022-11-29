﻿using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public abstract class PacketHandler<TRequest> : IPacketHandler
{
    private readonly IEncoder<Packet> _packetEncoder;
    protected Socket Sender { get; private set; } = null!;

    protected PacketHandler(IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(object state, CancellationToken ct)
    {
        var json = state.ToString();

        if (json is null)
        {
            await SendErrorAsync("State is not provided", ct);
            return;
        }
        
        var request = JsonDocument.Parse(json).Deserialize<TRequest>();
        
        if (request is null)
        {
            await SendErrorAsync("Bad request", ct);
            return;
        }
        
        await HandleAsync(request, ct);
    }
    
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);

    protected async Task SendResponseAsync(Packet response, CancellationToken ct)
    {
        if (Sender is null)
            throw new ApplicationException("Socket is not scoped");
        
        var request = _packetEncoder.Encode(response);
        _ = await Sender.SendAsync(request, SocketFlags.None, ct);
    }

    protected async Task SendErrorAsync(string message, CancellationToken ct)
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

    public void BeginSocketScope(Socket socket)
    {
        Sender = socket;
    }

    public void EndSocketScope()
    {
        Sender = null;
    }
}