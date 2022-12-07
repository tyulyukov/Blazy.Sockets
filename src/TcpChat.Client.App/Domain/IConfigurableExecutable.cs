namespace TcpChat.Client.App.Domain;

public interface IConfigurableExecutable : IExecutable
{
    public void Configure();
}