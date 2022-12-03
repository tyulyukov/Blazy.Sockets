namespace TcpChat.Console.Dto;

public class SendMessageRequest
{
    public string Chat { get; init; } = default!;
    public string Message { get; init; } = default!;
}