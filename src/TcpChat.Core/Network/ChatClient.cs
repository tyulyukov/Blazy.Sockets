using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;

namespace TcpChat.Core.Network;

internal class ChatClient : INetworkClient
{
    public bool Connected => _client.Connected;
    
    private readonly IPEndPoint _endPoint; 
    private readonly Socket _client;
    private readonly IEncoder<Packet> _packetEncoder;

    public ChatClient(IConfiguration configuration, IEncoder<Packet> packetEncoder)
    {
        var ip = IPAddress.Parse(configuration.GetValue<string>("Connection:IPAddress"));
        var port = configuration.GetValue<int>("Connection:Port");
        
        _packetEncoder = packetEncoder;
        _endPoint = new IPEndPoint(ip, port);
        _client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _client.ConnectAsync(_endPoint, ct);
    }

    public void Disconnect()
    {
        if (_client.Connected)
            _client.Shutdown(SocketShutdown.Both);
    }

    public async Task<Packet?> SendWithTimeOutAsync(Packet packet, TimeSpan timeout, CancellationToken ct = default)
    {
        await SendRequestAsync(packet, ct);
        return await ReceiveResponseWithTimeOutAsync(timeout, ct);
    }
    
    public async Task<Packet?> SendAsync(Packet packet, CancellationToken ct = default)
    {
        await SendRequestAsync(packet, ct);
        return await ReceiveResponseAsync(ct);
    }

    public async Task SendRequestAsync(Packet packet, CancellationToken ct = default)
    {
        try
        {
            var request = _packetEncoder.Encode(packet);
            _ = await _client.SendAsync(request, SocketFlags.None, ct);
        }
        catch
        {
            Disconnect();
            throw new SocketDisconnectedException();
        }
    }

    public async Task<Packet?> ReceiveResponseWithTimeOutAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            var buffer = new byte[1024];
            var received = await _client.ReceiveAsync(buffer, SocketFlags.None, cts.Token);
            return _packetEncoder.Decode(buffer, received);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            Disconnect();
            throw new SocketDisconnectedException("Server socket disconnected");
        }
    }
    
    public async Task<Packet?> ReceiveResponseAsync(CancellationToken ct = default)
    {
        try
        {
            var buffer = new byte[1024];
            var received = await _client.ReceiveAsync(buffer, SocketFlags.None, ct);
            return _packetEncoder.Decode(buffer, received);
        }
        catch
        {
            Disconnect();
            throw new SocketDisconnectedException("Server socket disconnected");
        }
    }
    
    public void Dispose()
    {
        _client.Close();
    }
}