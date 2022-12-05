namespace TcpChat.Console.Domain;

public interface IConfigurableExecutable : IExecutable
{
    void Configure();
}