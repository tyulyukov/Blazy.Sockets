using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class NetworkBuilder
{
    public const string ConnectedEventName = "Connection";
    public const string DisconnectedEventName = "Disconnection";

    private readonly List<string> _packetHandlerEvents;
    private readonly ContainerBuilder _builder;
    

    public NetworkBuilder() : this("appsettings.json") { }

    public NetworkBuilder(string configPath)
    {
        _builder = new ();
        _packetHandlerEvents = new List<string>();

        var config = new ConfigurationBuilder()
            .AddJsonFile(configPath, true, true)
            .Build();
        
        _builder.RegisterInstance(config).As<IConfiguration>();
        _builder.RegisterModule(new ConfigurationModule(config));
        
        Use<INetworkServer, ChatServer>();
        Use<ISocketAcceptor, SocketAcceptor>();
        Use<INetworkClient, ChatClient>();
    }

    public void Use<TInterface, TImplementation>() where TInterface : notnull where TImplementation : TInterface
    {
        _builder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
    }

    public void Use<TImplementation>() where TImplementation : notnull
    {
        _builder.RegisterType<TImplementation>().AsSelf().SingleInstance();
    }

    public void Use<TInstance>(TInstance instance) where TInstance : class
    {
        _builder.RegisterInstance(instance).AsSelf().SingleInstance();
    }

    public void UsePacketHandler<THandler>(string eventName) where THandler : IPacketHandler
    {
        if (_packetHandlerEvents.Contains(eventName))
            throw new ApplicationException($"Event handler with event name {eventName} already exists");

        _builder.RegisterType<THandler>()
            .Named<IPacketHandler>(eventName)
            .SingleInstance();

        _packetHandlerEvents.Add(eventName);
    }

    public void UseMiddleware<TMiddleware>() where TMiddleware : IMiddleware
    {
        _builder.RegisterType<TMiddleware>().As<IMiddleware>().SingleInstance();
    }
    
    public void UseConnectionHandler<THandler>() where THandler : PacketHandler<ConnectionDetails>
    {
        if (_packetHandlerEvents.Contains(ConnectedEventName))
            throw new ApplicationException("Connection handler already exists");

        _builder.RegisterType<THandler>()
            .Named<PacketHandler<ConnectionDetails>>(ConnectedEventName)
            .SingleInstance();
        
        _packetHandlerEvents.Add(ConnectedEventName);
    }

    public void UseDisconnectionHandler<THandler>() where THandler : PacketHandler<DisconnectionDetails>
    {
        if (_packetHandlerEvents.Contains(DisconnectedEventName))
            throw new ApplicationException("Disconnection handler already exists");

        _builder.RegisterType<THandler>()
            .Named<PacketHandler<DisconnectionDetails>>(DisconnectedEventName)
            .SingleInstance();
        
        _packetHandlerEvents.Add(DisconnectedEventName);
    }
    
    protected virtual void BeforeBuild()
    {
        _builder.RegisterType<JsonPacketEncoder>()
            .As<IEncoder<Packet>>()
            .SingleInstance()
            .IfNotRegistered(typeof(IEncoder<Packet>));
    }
    
    public IContainer Build()
    {
        BeforeBuild();
        return _builder.Build();
    }
}