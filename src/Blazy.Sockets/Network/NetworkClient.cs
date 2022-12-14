using System.Net;
using System.Net.Sockets;
using Blazy.Sockets.Contracts;
using Blazy.Sockets.Encoding;
using Blazy.Sockets.Exceptions;
using Blazy.Sockets.Handlers;
using Microsoft.Extensions.Configuration;

namespace Blazy.Sockets.Network;

internal class NetworkClient : INetworkClient
{
    public bool Connected => _connected;
    public EndPoint? RemoteEndPoint => _remoteEp;
    public EndPoint? LocalEndPoint => _client.LocalEndPoint;

    private bool _connected;
    private EndPoint? _remoteEp; 
    private readonly Socket _client;
    private readonly IEncoder _encoder;

    public NetworkClient(IConfiguration configuration, IEncoder encoder)
    {
        var ip = IPAddress.Parse(configuration.GetValue<string>("Connection:IPAddress"));
        var port = configuration.GetValue<int>("Connection:Port");

        _connected = false;
        _encoder = encoder;
        _remoteEp = new IPEndPoint(ip, port);
        _client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public NetworkClient(EndPoint remoteEp, IEncoder encoder)
    {
        _connected = false;
        _remoteEp = remoteEp;
        _client = new Socket(_remoteEp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _encoder = encoder;
    }

    public NetworkClient(Socket client, IEncoder encoder)
    {
        _connected = false;
        _client = client;
        _remoteEp = _client.RemoteEndPoint;
        _encoder = encoder;
    }
    
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_remoteEp is null)
            throw new ArgumentNullException(nameof(_remoteEp), "Remote End Point is not set");
        
        await _client.ConnectAsync(_remoteEp, ct);
        _connected = true;
    }

    public void Disconnect()
    {
        try
        {
            _client.Shutdown(SocketShutdown.Both);
        }
        finally
        {
            _connected = false;
        }
    }

    private void DisconnectInternal()
    {
        Disconnect();
        throw new SocketDisconnectedException("Server socket disconnected");
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
            var request = _encoder.Encode(packet);
            _ = await _client.SendAsync(request, SocketFlags.None, ct);
        }
        catch
        {
            DisconnectInternal();
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
            return _encoder.Decode<Packet>(buffer, received);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            DisconnectInternal();
            return null;
        }
    }
    
    public async Task<Packet?> ReceiveResponseAsync(CancellationToken ct = default)
    {
        try
        {
            var buffer = new byte[1024];
            var received = await _client.ReceiveAsync(buffer, SocketFlags.None, ct);
            return _encoder.Decode<Packet>(buffer, received);
        }
        catch
        {
            DisconnectInternal();
            return null;
        }
    }
    
    public void Dispose()
    {
        _client.Close();
    }
}