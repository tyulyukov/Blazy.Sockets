namespace TcpChat.Core.Handlers;

public interface IHandlersCollection
{
    void Register(string eventName, IPacketHandler handler);
    IPacketHandler? Resolve(string eventName);
}