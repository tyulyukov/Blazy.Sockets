using System.Text;
using System.Text.Json;
using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public class JsonPacketEncoder : IEncoder<Packet>
{
    public byte[] Encode(Packet value)
    {
        // value.State = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(value.State)) ?? throw new InvalidOperationException();
        var json = JsonSerializer.Serialize(value);
        return Encoding.UTF8.GetBytes(json);
    }

    public Packet? Decode(byte[] buffer, int length)
    {
        var responseJson = Encoding.UTF8.GetString(buffer, 0, length);
        return JsonSerializer.Deserialize<Packet>(responseJson);
    }
}