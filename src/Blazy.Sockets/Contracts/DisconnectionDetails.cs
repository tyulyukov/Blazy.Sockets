namespace Blazy.Sockets.Contracts;

public class DisconnectionDetails
{
    public DateTime DisconnectedAt { get; init; }
    public TimeSpan ConnectionTime { get; init; }
}