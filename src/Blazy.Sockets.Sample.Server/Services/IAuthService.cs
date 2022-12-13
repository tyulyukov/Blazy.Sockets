using Blazy.Sockets.Network;
using Blazy.Sockets.Sample.Server.Models;

namespace Blazy.Sockets.Sample.Server.Services;

public interface IAuthService
{
    bool Authenticate(User user);
    bool LogOut(string userName);
    User? FindBySender(INetworkClient socket);
}
