using System.Net;
using System.Net.Sockets;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;

namespace TcpChat.Core.Network;

public class ChatClient : INetworkClient
{
    public bool Connected => _client.Connected;
    
    private readonly IPEndPoint _endPoint; 
    private readonly Socket _client;
    private readonly IEncoder<Packet> _packetEncoder;

    public ChatClient(IPAddress ip, int remotePort, IEncoder<Packet> packetEncoder)
    {
        _packetEncoder = packetEncoder;
        _endPoint = new IPEndPoint(ip, remotePort);
        _client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync(CancellationToken ct)
    {
        await _client.ConnectAsync(_endPoint, ct);
    }

    public void Disconnect()
    {
        if (_client.Connected)
            _client.Shutdown(SocketShutdown.Both);
    }

    public async Task<Packet?> SendAsync(Packet packet, CancellationToken ct)
    {
        await SendRequestAsync(packet, ct);
        return await ReceiveResponseAsync(ct);
    }

    public async Task SendRequestAsync(Packet packet, CancellationToken ct)
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

    public async Task<Packet?> ReceiveResponseAsync(CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(1)); // TODO get this from configuration

            var buffer = new byte[8192];
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
    
    public void Dispose()
    {
        _client.Close();
    }
}