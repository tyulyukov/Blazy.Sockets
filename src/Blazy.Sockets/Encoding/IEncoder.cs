namespace Blazy.Sockets.Encoding;

public interface IEncoder
{
    byte[] Encode<TValue>(TValue value);
    TValue? Decode<TValue>(byte[] buffer, int length);
}