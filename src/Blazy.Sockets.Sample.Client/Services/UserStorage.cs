using Blazy.Sockets.Sample.Client.Models;

namespace Blazy.Sockets.Sample.Client.Services;

public class UserStorage : IUserStorage
{
    public User? CurrentUser => _user;
    private User? _user;

    public void Authenticate(User user)
    {
        _user = user;
    }

    public void LogOut()
    {
        _user = null;
    }

    public void JoinChat(Chat chat)
    {
        SetChatInternal(chat);
    }

    public void LeaveChat()
    {
        SetChatInternal(null);
    }

    private void SetChatInternal(Chat? chat)
    {
        if (_user is null)
            throw new ApplicationException("User is not authenticated");

        _user.CurrentChat = chat;
    }
}