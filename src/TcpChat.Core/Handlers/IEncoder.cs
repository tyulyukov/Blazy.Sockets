namespace TcpChat.Core.Handlers;

public interface IEncoder<TValue>
{
    byte[] Encode(TValue value);
    TValue? Decode(byte[] buffer, int length);
}