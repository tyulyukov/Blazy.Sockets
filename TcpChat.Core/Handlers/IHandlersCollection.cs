namespace TcpChat.Core.Handlers;

public interface IHandlersCollection
{
    void Register(string eventName, PacketHandler handler);
    PacketHandler? Resolve(string eventName);
}