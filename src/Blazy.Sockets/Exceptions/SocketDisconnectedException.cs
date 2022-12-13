namespace Blazy.Sockets.Exceptions;

public class SocketDisconnectedException : Exception
{
    public SocketDisconnectedException(string? message = null) : base(message) { }
}