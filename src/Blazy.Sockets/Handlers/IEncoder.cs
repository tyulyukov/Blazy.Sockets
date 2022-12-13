namespace Blazy.Sockets.Handlers;

public interface IEncoder<TValue>
{
    byte[] Encode(TValue value);
    TValue? Decode(byte[] buffer, int length);
}