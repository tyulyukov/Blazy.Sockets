using System.Text.Json.Serialization;

namespace Blazy.Sockets.Sample.Server.Models;

public class Chat
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public User Creator { get; set; } = default!;
    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}