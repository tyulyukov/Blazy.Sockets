using System.Net.Sockets;

namespace TcpChat.Core.Network;

public interface ISocketAcceptor : IDisposable
{
    Task AcceptSocketAsync(Socket socket, CancellationToken ct);
}