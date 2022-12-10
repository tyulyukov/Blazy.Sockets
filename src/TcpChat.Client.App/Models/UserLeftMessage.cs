namespace TcpChat.Client.App.Models;

public class UserLeftMessage
{
    public string User { get; set; } = default!;
    public string Chat { get; set; } = default!;
    public bool Disconnected { get; set; }
}