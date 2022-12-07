using Autofac;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class ChatServerBuilder : NetworkBuilder
{
    private class NetworkApplication : NetworkApplication<INetworkServer>
    {
        internal NetworkApplication(IContainer container) : base(container) { }
        
        public override INetworkServer Resolve()
        {
            return Container.Resolve<INetworkServer>();
        }
    }
    
    private const string ConnectedEventName = "Connection";
    private const string DisconnectedEventName = "Disconnection";

    private readonly List<string> _packetHandlerEvents;
    
    public ChatServerBuilder()
    {
        _packetHandlerEvents = new List<string>();
        
        Builder.RegisterType<ChatServer>().As<INetworkServer>();
        Builder.RegisterType<SocketAcceptor>().As<ISocketAcceptor>();
    }

    public void UsePacketHandler<THandler>(string eventName) where THandler : IPacketHandler
    {
        if (_packetHandlerEvents.Contains(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");
        
        Builder.RegisterType<THandler>().As<IPacketHandler>().SingleInstance().WithParameter("eventName", eventName);
        
        _packetHandlerEvents.Add(eventName);
    }

    public void UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware
    {
        Builder.RegisterType<TMiddleware>().SingleInstance().As<IMiddleware>();
    }
    
    public void UseConnectionHandler<THandler>() where THandler : PacketHandler<ConnectionDetails>
    {
        if (_packetHandlerEvents.Contains(ConnectedEventName))
            throw new ApplicationException("Connection handler already exists");
        
        Builder.RegisterType<THandler>().AsSelf().SingleInstance().WithParameter("eventName", ConnectedEventName);
        
        _packetHandlerEvents.Add(ConnectedEventName);
    }

    public void UseDisconnectionHandler<THandler>() where THandler : PacketHandler<DisconnectionDetails>
    {
        if (_packetHandlerEvents.Contains(DisconnectedEventName))
            throw new ApplicationException("Disconnection handler already exists");
        
        Builder.RegisterType<THandler>().AsSelf().SingleInstance().WithParameter("eventName", DisconnectedEventName);
        
        _packetHandlerEvents.Add(DisconnectedEventName);
    }

    public NetworkApplication<INetworkServer> Build()
    {
        BeforeBuild();
        
        var container = Builder.Build();
        return new NetworkApplication(container);
    }
}