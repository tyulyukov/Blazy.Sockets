using Autofac;
using HashidsNet;
using TcpChat.Core.Application;
using TcpChat.Core.Logging;
using TcpChat.Core.Network;
using TcpChat.Server.App.Handlers;
using TcpChat.Server.App.Services;

var builder = new NetworkBuilder();
builder.Use<ILogHandler, LogHandler>();
builder.Use(new Hashids(DateTime.UtcNow.ToString(), 5));

builder.Use<IChatService, ChatService>();
builder.Use<IAuthService, AuthService>();

builder.UsePacketHandler<CreateChatHandler>("Create Chat");
builder.UsePacketHandler<ConnectToChatHandler>("Connect To Chat");
builder.UsePacketHandler<LeaveChatHandler>("Leave Chat");
builder.UsePacketHandler<AuthHandler>("Auth");
builder.UsePacketHandler<SendMessageHandler>("Message");

builder.UseConnectionHandler<ConnectionHandler>();
builder.UseDisconnectionHandler<DisconnectionHandler>();

await using var app = builder.Build();
var server = app.Resolve<INetworkServer>();

using var cts = new CancellationTokenSource();

void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs) => cts.Cancel();

Console.CancelKeyPress += ConsoleOnCancelKeyPress;

try
{
    await server.RunAsync(cts.Token);
}
catch (Exception exception)
{
    Console.Error.Write(exception);
}

Console.CancelKeyPress -= ConsoleOnCancelKeyPress;