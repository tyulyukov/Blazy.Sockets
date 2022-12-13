using System.Text.Json.Serialization;

namespace Blazy.Sockets.Sample.Client.Models;

public class Message
{
    public string From { get; set; } = default!;
    public string Chat { get; set; } = default!;
    [JsonPropertyName("Message")]
    public string Content { get; set; } = default!;
}