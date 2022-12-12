namespace TcpChat.Client.App.Models;

public class User
{
    public string Name { get; set; } = default!;
    public Chat? CurrentChat { get; set; }
}