namespace TcpChat.Console.Models;

public class Chat
{
    public string Name { get; init; } = default!;
    public User Creator { get; init; } = default!;
    public ICollection<User> Users { get; init; } = default!;
}