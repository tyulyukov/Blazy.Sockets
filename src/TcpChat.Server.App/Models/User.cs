using System.Net.Sockets;

namespace TcpChat.Server.App.Models;

public class User
{
    public string Name { get; set; } = default!;
    public Socket Socket { get; set; } = default!;
}