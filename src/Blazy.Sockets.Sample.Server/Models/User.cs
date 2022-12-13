using Blazy.Sockets.Network;

namespace Blazy.Sockets.Sample.Server.Models;

public class User
{
    public string Name { get; set; } = default!;
    public INetworkClient Client { get; set; } = default!;
}