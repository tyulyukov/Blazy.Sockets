using Blazy.Sockets.Contracts;
using Blazy.Sockets.Handlers;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Sample.Server.Dto;
using Blazy.Sockets.Sample.Server.Services;
using Serilog;

namespace Blazy.Sockets.Sample.Server.Handlers;

public class DisconnectionHandler : PacketHandler<DisconnectionDetails>
{
    private readonly ILogger _logger;
    private readonly IAuthService _authService;
    private readonly IChatService _chatService;

    public DisconnectionHandler(IEncoder<Packet> packetEncoder, ILogger logger, IAuthService authService, IChatService chatService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
        _chatService = chatService;
    }

    public override async Task HandleAsync(DisconnectionDetails details, CancellationToken ct)
    {
        var sender = _authService.FindBySender(Sender);
        
        if (sender is not null)
        {
            _authService.LogOut(sender.Name);
            
            var leftChats = _chatService.LeaveAllChats(sender);

            foreach (var leftChat in leftChats)
            {
                foreach (var user in _chatService.GetUsersFromChat(leftChat.Id, u => u.Name != sender.Name))
                {
                    await SendResponseAsync(user.Client, new Packet
                    {
                        Event = "User Left",
                        State = new UserLeftChatDto
                        {
                            User = sender.Name,
                            Chat = leftChat.Id,
                            Disconnected = true
                        }
                    }, ct);
                }
            }
            
            switch (leftChats.Count)
            {
                case 0:
                    _logger.Information("{Username} was not connected to any chat while disconnecting", sender.Name);
                    break;
                case 1:
                    _logger.Information("{Username} left {ChatName} chat due to disconnection", sender.Name, leftChats[0].Name);
                    break;
                case > 1:
                    _logger.Information("{Username} left {ChatsCount} chats due to disconnection", sender.Name, leftChats.Count);
                    break;
            }
            
            _logger.Information("Disconnected user {Username} Connection Elapsed: {ConnectionTime}", sender.Name, details.ConnectionTime);
        }
        else
        {
            _logger.Information("Disconnected {RemoteEndPoint} Connection Elapsed: {ConnectionTime}", Sender.RemoteEndPoint, details.ConnectionTime);
        }
    }
}