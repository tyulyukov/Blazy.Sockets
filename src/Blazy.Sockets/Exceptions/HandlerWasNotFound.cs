namespace Blazy.Sockets.Exceptions;

public class HandlerWasNotFound : ApplicationException
{
    public HandlerWasNotFound(string? message = null) : base(message) { }
}