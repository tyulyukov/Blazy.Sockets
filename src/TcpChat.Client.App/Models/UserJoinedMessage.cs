namespace TcpChat.Client.App.Models;

public class UserJoinedMessage
{
    public string User { get; set; } = default!;
    public string Chat { get; set; } = default!;
}