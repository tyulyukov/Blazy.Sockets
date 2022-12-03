namespace TcpChat.Console.Services;

public interface IServerCommandParserService
{
    bool TryParse(string commandString, out Command command);
}

// Temporarily here 
public struct Command
{
    
}