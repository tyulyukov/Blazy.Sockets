﻿using System.Net;
using System.Net.Sockets;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Handlers;
using TcpChat.Core.Logging;

namespace TcpChat.Core.Network;

public class ChatClient : INetworkClient
{
    private readonly IPEndPoint _endPoint; 
    private readonly Socket _client;
    private readonly ILogHandler _logger;
    private readonly IEncoder<Packet> _packetEncoder;

    public ChatClient(IPAddress ip, int remotePort, ILogHandler logger, IEncoder<Packet> packetEncoder)
    {
        _logger = logger;
        _packetEncoder = packetEncoder;
        _endPoint = new IPEndPoint(ip, remotePort);
        _client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public async Task ConnectAsync(CancellationToken ct)
    {
        await _client.ConnectAsync(_endPoint, ct);
        _logger.HandleText("Connected");
    }

    public void Disconnect()
    {
        _client.Shutdown(SocketShutdown.Both);
        _client.Close();
        _logger.HandleText("Disconnected");
    }

    /// <summary>
    /// Sends packet to the server
    /// </summary>
    /// <param name="packet">Request</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Response, it will not be null if server successfully handles it</returns>
    public async Task<Packet?> SendAsync(Packet packet, CancellationToken ct)
    {
        try
        {
            var request = _packetEncoder.Encode(packet);
            _ = await _client.SendAsync(request, SocketFlags.None, ct);

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
            throw new SocketDisconnectedException();
        }
    }

    public void Dispose()
    {
       Disconnect();
    }
}