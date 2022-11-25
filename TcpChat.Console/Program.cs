using System.Net;
using TcpChat.Core;
using TcpChat.Core.Contracts;
using TcpChat.Core.Network;

var cts = new CancellationTokenSource();
var token = cts.Token;

// var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
using var client = new ChatClient(IPAddress.Parse("127.0.0.1"), 7777);

var handlers = new HandlersCollection();
using var server = new ChatServer(new IPEndPoint(IPAddress.Any, 7777), handlers, token);

var task = Task.Run(server.Run, token);

client.Connect();

Task.Delay(5000).ContinueWith(_ =>
{
    cts.Cancel();
});
// Task.Delay(6000).ContinueWith(_ => { client.Disconnect(); });
task.ContinueWith(_ =>
{
    client.Send(new Packet());
}).Wait();
