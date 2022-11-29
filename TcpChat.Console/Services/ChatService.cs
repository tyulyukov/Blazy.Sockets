using HashidsNet;
using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public class ChatService : IChatService
{
    private readonly Dictionary<int, Chat?> _chats; // TODO in-memory database instead of this sh1t
    private readonly Hashids _hashids;
    
    private int _lastId;

    public ChatService(Hashids hashids)
    {
        _hashids = hashids;
        _chats = new();
    }

    public string CreateChat(Chat? chat)
    {
        var id = _lastId;
        _chats.Add(id, chat);

        _lastId++;

        return _hashids.Encode(id);
    }

    public bool DeleteChat(string id, User user)
    {
        var key = _hashids.Decode(id)[0];
        if (!_chats.TryGetValue(key, out var chat))
            return false;

        return chat.Creator.Name == user.Name && _chats.Remove(key);
    }
    
    public bool JoinChat(string id, User user)
    {
        if (!_chats.TryGetValue(_hashids.Decode(id)[0], out var chat))
            return false;
        
        chat.Users.Add(user);
        
        return true;
    }

    public bool LeaveChat(string id, User user)
    {
        return _chats.TryGetValue(_hashids.Decode(id)[0], out var chat) && chat.Users.Remove(user);
    }

    public bool KickUserFromChat(string id, string userName, User user)
    {
        var key = _hashids.Decode(id)[0];
        if (!_chats.TryGetValue(key, out var chat))
            return false;

        if (chat.Creator.Name != user.Name)
            return false;
        
        User? userToDelete = null;

        foreach (var chatUser in chat.Users)
        {
            if (chatUser.Name != userName)
                continue;
            
            userToDelete = chatUser;
            break;
        }

        if (userToDelete is null)
            return false;
            
        return chat.Users.Remove(userToDelete);
    }
}