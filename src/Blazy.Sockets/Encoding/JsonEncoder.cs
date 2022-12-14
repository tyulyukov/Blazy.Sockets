using System.Text.Json;

namespace Blazy.Sockets.Encoding;

public class JsonEncoder : IEncoder
{
    public byte[] Encode<TValue>(TValue value)
    {
        var json = JsonSerializer.Serialize(value);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public TValue? Decode<TValue>(byte[] buffer, int length)
    {
        var responseJson = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        return JsonSerializer.Deserialize<TValue>(responseJson);
    }
}