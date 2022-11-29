using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public class AuthService : IAuthService
{
    private readonly List<User> _users; // TODO in-memory database instead of this sh1t

    public AuthService()
    {
        _users = new();
    }

    public bool Authenticate(User user)
    {
        throw new NotImplementedException();
    }
}