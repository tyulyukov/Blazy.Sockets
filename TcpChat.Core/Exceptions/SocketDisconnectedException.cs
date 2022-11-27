using System.Net.Sockets;

namespace TcpChat.Core.Exceptions;

public class SocketDisconnectedException : Exception
{
    public SocketDisconnectedException(string? message = null) : base(message) { }
}