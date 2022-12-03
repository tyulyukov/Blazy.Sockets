using System.Collections.Concurrent;
using HashidsNet;
using TcpChat.Console.Domain;
using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public class ChatService : IChatService
{
    private readonly ConcurrentDictionary<int, Chat> _chats; // TODO in-memory database instead of this sh1t
    private readonly Hashids _hashids;
    private readonly AutoIncrement _id;

    public ChatService(Hashids hashids)
    {
        _hashids = hashids;
        _id = new();
        _chats = new();
    }

    public string? CreateChat(Chat chat)
    {
        var id = _id.Value;
        return !_chats.TryAdd(id, chat) ? null : _hashids.Encode(id);
    }

    public bool DeleteChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var key))
            return false;
        
        if (!_chats.TryGetValue(key, out var chat))
            return false;

        return chat.Creator.Name == user.Name && _chats.TryRemove(key, out _);
    }
    
    public bool JoinChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var id))
            return false;
        
        if (!_chats.TryGetValue(id, out var chat))
            return false;
        
        chat.Users.Add(user);
        
        return true;
    }

    public bool LeaveChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var id))
            return false;
        
        return _chats.TryGetValue(id, out var chat) && chat.Users.Remove(user);
    }

    public bool KickUserFromChat(string hashId, string userName, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var key))
            return false;
        
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

        return userToDelete is not null && chat.Users.Remove(userToDelete);
    }

    public User[] GetUsersFromChat(string hashId, Func<User, bool> predicate)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var chatId))
            return Array.Empty<User>();
        
        return !_chats.TryGetValue(chatId, out var chat) 
            ? Array.Empty<User>() 
            : chat.Users.Where(predicate).ToArray();
    }
}