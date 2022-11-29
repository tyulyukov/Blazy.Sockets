using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public interface IAuthService
{
    bool Authenticate(User user);
}
