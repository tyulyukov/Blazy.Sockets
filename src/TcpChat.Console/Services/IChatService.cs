using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public interface IChatService
{
    string? CreateChat(Chat chat);
    bool DeleteChat(string id, User user);
    bool JoinChat(string id, User user);
    bool LeaveChat(string id, User user);
    bool KickUserFromChat(string id, string userName, User user);
}