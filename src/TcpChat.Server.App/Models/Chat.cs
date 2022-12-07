using System.Text.Json.Serialization;

namespace TcpChat.Server.App.Models;

public class Chat
{
    public string Name { get; set; } = default!;
    public User Creator { get; set; } = default!;
    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}