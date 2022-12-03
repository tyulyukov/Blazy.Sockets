using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public interface IChatService
{
    string? CreateChat(Chat chat);
    bool DeleteChat(string id, User user);
    bool JoinChat(string id, User user);
    bool LeaveChat(string hashId, User user);
    bool KickUserFromChat(string hashId, string userName, User user);
    User[] GetUsersFromChat(string chatId, Func<User, bool> predicate);
}