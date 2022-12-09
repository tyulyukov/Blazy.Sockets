using Autofac;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class ChatClientBuilder : NetworkBuilder
{
    public ChatClientBuilder()
    {
        Builder.RegisterType<ChatClient>().As<INetworkClient>().SingleInstance();
    }
}