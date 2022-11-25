using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;
using TcpChat.Core.Exceptions;
using TcpChat.Core.Interfaces;

namespace TcpChat.Core.Network;

public class ChatClient : IDisposable
{
    private readonly IPEndPoint _endPoint; 
    private readonly Socket _client;
    private readonly ILogHandler _logger;

    public ChatClient(IPAddress ip, int remotePort, ILogHandler logger)
    {
        _logger = logger;
        _endPoint = new IPEndPoint(ip, remotePort);
        _client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect()
    {
        _client.Connect(_endPoint);
    }

    public void Disconnect()
    {
        _client.Close();
    }

    /// <summary>
    /// Sends packet to the server
    /// </summary>
    /// <param name="packet">Request</param>
    /// <returns>Response, it will not be null if server successfully handles it</returns>
    public Packet? Send(Packet packet)
    {
        try
        {
            var stream = new NetworkStream(_client);
            JsonSerializer.Serialize(stream, packet);
            return JsonSerializer.Deserialize<Packet>(stream);
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