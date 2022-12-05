namespace TcpChat.Core.Contracts;

public class Packet
{
    public string Event { get; set; } = default!;
    public object State { get; set; } = default!;
}