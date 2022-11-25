using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Network;

public class ChatClient
{
    private readonly IPEndPoint _endPoint; 
    private readonly TcpClient _client;

    public ChatClient(IPAddress ip, int remotePort)
    {
        _endPoint = new IPEndPoint(ip, remotePort);
        _client = new TcpClient();
    }

    public void Connect()
    {
        if (_client.Connected)
            throw new ApplicationException("Chat Client is already connected");

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
        JsonSerializer.Serialize(_client.GetStream(), packet);
        return JsonSerializer.Deserialize<Packet>(_client.GetStream());
    }
}