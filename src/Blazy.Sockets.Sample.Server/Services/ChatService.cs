using System.Collections.Concurrent;
using Blazy.Sockets.Sample.Server.Domain;
using Blazy.Sockets.Sample.Server.Models;
using HashidsNet;

namespace Blazy.Sockets.Sample.Server.Services;

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
        chat.Id = _hashids.Encode(id);
        
        return !_chats.TryAdd(id, chat) ? null : chat.Id;
    }

    public bool DeleteChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var key))
            return false;
        
        if (!_chats.TryGetValue(key, out var chat))
            return false;

        return chat.Creator.Name == user.Name && _chats.TryRemove(key, out _);
    }
    
    public Chat? JoinChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var id))
            return null;
        
        if (!_chats.TryGetValue(id, out var chat))
            return null;
        
        chat.Users.Add(user);
        
        return chat;
    }

    public bool LeaveChat(string hashId, User user)
    {
        if (!_hashids.TryDecodeSingle(hashId, out var id))
            return false;
        
        return _chats.TryGetValue(id, out var chat) && chat.Users.Remove(user);
    }

    public List<Chat> LeaveAllChats(User user)
    {
        var chats = new List<Chat>();
        foreach (var chat in _chats.Values)
            if (chat.Users.Remove(user))
                chats.Add(chat);

        return chats;
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