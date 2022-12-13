using Autofac;
using Blazy.Sockets.Logging;
using Blazy.Sockets.Network;
using Blazy.Sockets.Sample.Server.Handlers;
using Blazy.Sockets.Sample.Server.Services;
using HashidsNet;

var builder = new NetworkBuilder();
builder.UseDefaultLogger();
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