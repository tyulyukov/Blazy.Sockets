using System.Net.Sockets;

namespace TcpChat.Console.Models;

public class User
{
    public string Name { get; set; } = default!;
    public Socket Socket { get; set; } = default!;
}