[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner-direct-single.svg)](https://stand-with-ukraine.pp.ua)

# Tcp Chat
`Small sockets framework with console chat sample`

Small event based sockets framework which works on Tcp protocol. I am inspired by socket.io and FastEndpoints. You can implement ur own Packet Handler (also for Connection and Disconnection)

How ridiculous it is, but i still cant send messages in my sample xD.

> 🚧 Project is still in progress... (ofc you can fork this project and build your sample)

![Main menu](https://github.com/tyulyukov/TcpChat/blob/master/docs/assets/1.png)
![Client](https://github.com/tyulyukov/TcpChat/blob/master/docs/assets/2.png)
![Server](https://github.com/tyulyukov/TcpChat/blob/master/docs/assets/3.png)

## Small example of TcpChat.Core:

#### 1. First, create packet handler with request dto (yeah they look like Endpoint)
```csharp
public class AuthRequest
{
    public string Username { get; init; } = default!;
}

public class AuthHandler : PacketHandler<AuthRequest>
{
    private readonly IAuthService _authService;

    public AuthHandler(IEncoder<Packet> packetEncoder, IAuthService authService) : base(packetEncoder)
    {
        _logger = logger;
        _authService = authService;
    }

    public override async Task HandleAsync(AuthRequest request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Username
        };

        var result = _authService.Authenticate(user);

        if (!result)
        {
            await SendErrorAsync("Username is already taken", ct);
            return;
        }
        
        await SendResponseAsync(new Packet
        {
            Event = "Authenticated",
            State = $"Authenticated as {user.Name}"
        }, ct);
    }
}
```


#### 2. Register packet handler in in Program.cs
```csharp
var builder = new ChatServerBuilder();

builder.Use<IAuthService, AuthService>();
builder.UsePacketHandler<AuthHandler>("Auth");

using var app = builder.Build();
var server = app.Resolve();
await server.RunAsync();
```

#### 3. Send packet with this event name. And that`s it!
```csharp
var builder = new ChatClientBuilder();
using var app = builder.Build();
var client = app.Resolve();

await client.ConnectAsync(ct);

var response = await client.SendAsync(new Packet
{
    Event = "Auth",
    State = new
    {
        Username = "tyulyukov"
    }
}, ct);
```

## 📈 Plans for:
- ILogger from MS instead of rough ILogHandler
- Middlewares
