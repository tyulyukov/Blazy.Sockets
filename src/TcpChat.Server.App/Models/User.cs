using TcpChat.Core.Network;

namespace TcpChat.Server.App.Models;

public class User
{
    public string Name { get; set; } = default!;
    public INetworkClient Client { get; set; } = default!;
}