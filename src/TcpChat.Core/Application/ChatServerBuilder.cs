using Autofac;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class ChatServerBuilder : NetworkBuilder
{
    public const string ConnectedEventName = "Connection";
    public const string DisconnectedEventName = "Disconnection";

    private readonly List<string> _packetHandlerEvents;
    
    public ChatServerBuilder()
    {
        _packetHandlerEvents = new List<string>();

        Use<INetworkServer, ChatServer>();
        Use<ISocketAcceptor, SocketAcceptor>();
    }

    public void UsePacketHandler<THandler>(string eventName) where THandler : IPacketHandler
    {
        if (_packetHandlerEvents.Contains(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");

        Builder.RegisterType<THandler>()
            .Named<IPacketHandler>(eventName)
            .SingleInstance();

        _packetHandlerEvents.Add(eventName);
    }

    public void UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware
    {
        Builder.RegisterType<TMiddleware>().As<IMiddleware>().SingleInstance();
    }
    
    public void UseConnectionHandler<THandler>() where THandler : PacketHandler<ConnectionDetails>
    {
        if (_packetHandlerEvents.Contains(ConnectedEventName))
            throw new ApplicationException("Connection handler already exists");

        Builder.RegisterType<THandler>()
            .Named<PacketHandler<ConnectionDetails>>(ConnectedEventName)
            .SingleInstance();
        
        _packetHandlerEvents.Add(ConnectedEventName);
    }

    public void UseDisconnectionHandler<THandler>() where THandler : PacketHandler<DisconnectionDetails>
    {
        if (_packetHandlerEvents.Contains(DisconnectedEventName))
            throw new ApplicationException("Disconnection handler already exists");

        Builder.RegisterType<THandler>()
            .Named<PacketHandler<DisconnectionDetails>>(DisconnectedEventName)
            .SingleInstance();
        
        _packetHandlerEvents.Add(DisconnectedEventName);
    }
}