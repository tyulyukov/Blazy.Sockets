using Autofac;
using TcpChat.Core.Network;

namespace TcpChat.Core.Application;

public class ChatClientBuilder : NetworkBuilder
{
    public ChatClientBuilder()
    {
        Use<INetworkClient, ChatClient>();
    }
}