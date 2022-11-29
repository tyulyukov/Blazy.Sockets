using System.Net;

namespace TcpChat.Console.Models;

public class User
{
    public string Name { get; init; } = default!;
    public IPEndPoint EndPoint { get; init; } = default!;
}