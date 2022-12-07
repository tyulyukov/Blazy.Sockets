using System.Net.Sockets;

namespace TcpChat.Client.App.Models;

public class User
{
    public string Name { get; set; } = default!;
    public Socket Socket { get; set; } = default!;
}