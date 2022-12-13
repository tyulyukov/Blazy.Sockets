namespace Blazy.Sockets.Sample.Client.Models;

public class User
{
    public string Name { get; set; } = default!;
    public Chat? CurrentChat { get; set; }
}