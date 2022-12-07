using Autofac;

namespace TcpChat.Core.Application;

public abstract class NetworkApplication<TApp> : IDisposable where TApp : notnull
{
    protected readonly IContainer Container;

    internal NetworkApplication(IContainer container)
    {
        Container = container;
    }
    
    public abstract TApp Resolve();

    public void Dispose()
    {
        Container.Dispose();
    }
}