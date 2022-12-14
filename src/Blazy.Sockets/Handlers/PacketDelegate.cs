using Blazy.Sockets.Contracts;
using Blazy.Sockets.Network;

namespace Blazy.Sockets.Handlers;

public delegate Task PacketDelegate(Packet request, INetworkClient sender, CancellationToken ct = default);