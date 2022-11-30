using System.Net;
using System.Net.Sockets;

namespace TcpChat.Console.Models;

public class User
{
    public string Name { get; init; } = default!;
    public Socket Socket { get; init; }
}