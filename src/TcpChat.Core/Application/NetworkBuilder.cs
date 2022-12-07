using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;
using TcpChat.Core.Contracts;
using TcpChat.Core.Handlers;

namespace TcpChat.Core.Application;

public abstract class NetworkBuilder
{
    protected readonly ContainerBuilder Builder;

    protected NetworkBuilder()
    {
        Builder = new ();

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        
        Builder.RegisterInstance(config).As<IConfiguration>();
        Builder.RegisterModule(new ConfigurationModule(config));
    }

    public void Use<TInterface, TImplementation>() where TInterface : notnull where TImplementation : TInterface
    {
        Builder.RegisterType<TImplementation>().As<TInterface>().SingleInstance();
    }

    public void Use<TImplementation>() where TImplementation : notnull
    {
        Builder.RegisterType<TImplementation>().AsSelf().SingleInstance();
    }

    public void Use<TInstance>(TInstance instance) where TInstance : class
    {
        Builder.RegisterInstance(instance).AsSelf().SingleInstance();
    }

    protected void BeforeBuild()
    {
        Builder.RegisterType<JsonPacketEncoder>().As<IEncoder<Packet>>().IfNotRegistered(typeof(IEncoder<Packet>));
    }
}