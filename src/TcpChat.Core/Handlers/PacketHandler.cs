﻿using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public abstract class PacketHandler<TRequest> : IPacketHandler where TRequest : notnull, new()
{
    private readonly IEncoder<Packet> _packetEncoder;
    protected Socket Sender { get; private set; } = null!;

    protected PacketHandler(IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
    }

    public async Task ExecuteAsync(object state, Socket sender, CancellationToken ct)
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
        
        await HandleWithScopedSocketAsync(sender, request, ct);
    }

    public abstract Task HandleAsync(TRequest request, CancellationToken ct);
    
    protected async Task SendResponseAsync(Packet response, CancellationToken ct)
    {
        await SendResponseAsync(Sender, response, ct);
    }
    
    protected async Task SendResponseAsync(Socket client, Packet response, CancellationToken ct)
    {
        var request = _packetEncoder.Encode(response);
        _ = await client.SendAsync(request, SocketFlags.None, ct);
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

    public async Task HandleWithScopedSocketAsync(Socket sender, TRequest request, CancellationToken ct)
    {
        Sender = sender;
        await HandleAsync(request, ct);
        Sender = null;
    }
}