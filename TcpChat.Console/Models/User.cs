using System.Net;

namespace TcpChat.Console.Models;

public class User
{
    public string Name { get; init; } = default!;
    public EndPoint? EndPoint { get; init; } = default!;
}