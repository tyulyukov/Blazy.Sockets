namespace TcpChat.Console.Executables;

public interface IConfigurableExecutable : IExecutable
{
    void Configure();
}