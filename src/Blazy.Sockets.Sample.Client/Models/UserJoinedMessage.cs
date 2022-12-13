namespace Blazy.Sockets.Sample.Client.Models;

public class UserJoinedMessage
{
    public string User { get; set; } = default!;
    public string Chat { get; set; } = default!;
}