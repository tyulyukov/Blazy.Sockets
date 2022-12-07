namespace TcpChat.Client.App.Services;

public class ServerCommandParserService : IServerCommandParserService
{
    public bool TryParse(string commandString, out Command command)
    {
        return false;
    }
}