using Blazy.Sockets.Sample.Server.Models;

namespace Blazy.Sockets.Sample.Server.Services;

public interface IChatService
{
    string? CreateChat(Chat chat);
    bool DeleteChat(string id, User user);
    Chat? JoinChat(string id, User user);
    bool LeaveChat(string hashId, User user);
    List<Chat> LeaveAllChats(User user);
    bool KickUserFromChat(string hashId, string userName, User user);
    User[] GetUsersFromChat(string chatId, Func<User, bool> predicate);
}