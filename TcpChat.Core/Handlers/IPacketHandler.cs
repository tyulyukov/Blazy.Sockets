using TcpChat.Core.Contracts;

namespace TcpChat.Core.Handlers;

public interface IPacketHandler
{
    Task ExecuteAsync(object state, CancellationToken ct);
    void BeginSocketScope(System.Net.Sockets.Socket socket);
    void EndSocketScope();
}