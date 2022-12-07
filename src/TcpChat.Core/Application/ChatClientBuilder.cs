using Autofac;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class ChatClientBuilder : NetworkBuilder
{
    private class NetworkApplication : NetworkApplication<INetworkClient>
    {
        internal NetworkApplication(IContainer container) : base(container) { }
        
        public override INetworkClient Resolve()
        {
            return Container.Resolve<INetworkClient>();
        }
    }

    public ChatClientBuilder()
    {
        Builder.RegisterType<ChatClient>().As<INetworkClient>();
    }
    
    public NetworkApplication<INetworkClient> Build()
    {
        BeforeBuild();
        
        var container = Builder.Build();
        return new NetworkApplication(container);
    }
}