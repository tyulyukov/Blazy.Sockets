using System.Net.Sockets;
using TcpChat.Server.App.Models;

namespace TcpChat.Server.App.Services;

public interface IAuthService
{
    bool Authenticate(User user);
    bool LogOut(string userName);
    User? FindBySocket(Socket socket);
}
