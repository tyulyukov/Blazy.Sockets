using Blazy.Sockets.Sample.Client.Models;

namespace Blazy.Sockets.Sample.Client.Services;

public interface IUserStorage
{
    User? CurrentUser { get; }
    void Authenticate(User user);
    void LogOut();
    void JoinChat(Chat chat);
    void LeaveChat();
}