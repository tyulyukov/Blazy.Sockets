using TcpChat.Core.Interfaces;

namespace TcpChat.Core;

public interface IHandlersCollection
{
    void Register(string eventName, IPacketHandler handler);
    IPacketHandler? Resolve(string eventName);
}