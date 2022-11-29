using TcpChat.Console.Models;

namespace TcpChat.Console.Services;

public interface IChatService
{
    string CreateChat(Chat? chat);
}