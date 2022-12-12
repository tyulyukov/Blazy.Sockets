using TcpChat.Client.App.Models;

namespace TcpChat.Client.App.Services;

public interface IUserStorage
{
    User? CurrentUser { get; }
    void Authenticate(User user);
    void LogOut();
    void JoinChat(Chat chat);
    void LeaveChat();
}