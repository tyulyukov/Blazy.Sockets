using System.Net.Sockets;
using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public interface IAuthService
{
    bool Authenticate(User user);
    bool LogOut(string userName);
    User? FindBySocket(Socket socket);
}
