using TcpChat.Core.Network;
using TcpChat.Server.App.Models;

namespace TcpChat.Server.App.Services;

public interface IAuthService
{
    bool Authenticate(User user);
    bool LogOut(string userName);
    User? FindBySender(INetworkClient socket);
}
