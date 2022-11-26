using System.Text;
using System.Text.Json;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public class JsonPacketEncoder : IEncoder<Packet>
{
    public byte[] Encode(Packet value)
    {
        var requestJson = JsonSerializer.Serialize(value);
        return Encoding.UTF8.GetBytes(requestJson);
    }

    public Packet? Decode(byte[] buffer, int length)
    {
        var responseJson = Encoding.UTF8.GetString(buffer, 0, length);
        return JsonSerializer.Deserialize<Packet>(responseJson);
    }
}