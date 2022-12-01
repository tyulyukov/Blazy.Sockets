namespace TcpChat.Core.Contracts;

public class Packet
{
    public string Event { get; init; } = default!;
    public object State { get; init; } = default!;
}