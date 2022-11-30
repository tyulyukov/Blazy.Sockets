﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public class AuthService : IAuthService
{
    private readonly ConcurrentDictionary<string, User> _users; // TODO in-memory database instead of this sh1t
    private readonly object _locker;

    public AuthService()
    {
        _locker = new();
        _users = new();
    }

    public bool Authenticate(User user)
    {
        return !_users.ContainsKey(user.Name) && _users.TryAdd(user.Name, user);
    }

    public bool LogOut(string userName)
    {
        return _users.TryRemove(userName, out _);
    }

    public User? FindBySocket(Socket socket)
    {
        try
        {
            var keyValuePair = _users.FirstOrDefault(u => u.Value.Socket == socket);
            return keyValuePair.Value;
        }
        catch
        {
            return null;
        }
    }
}