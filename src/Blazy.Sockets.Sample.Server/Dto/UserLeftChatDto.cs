namespace Blazy.Sockets.Sample.Server.Dto;

public class UserLeftChatDto
{
    public string User { get; set; } = default!;
    public string Chat { get; set; } = default!;
    public bool Disconnected { get; set; }
}