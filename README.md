[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner-direct-single.svg)](https://stand-with-ukraine.pp.ua)

# Blazy Sockets
`Sockets Framework for building modern apps`

Event-based sockets framework (TCP only). Actually, I was inspired by socket.io and FastEndpoints so there are loads of similarities.
Full documentation would be published soon

> 🚧 Project is still in progress... (ofc you can fork this project and build your sample)

![Main menu](https://github.com/tyulyukov/Blazy.Sockets/blob/master/docs/assets/main1.png)
![Client](https://github.com/tyulyukov/Blazy.Sockets/blob/master/docs/assets/chat1.png)
![Server](https://github.com/tyulyukov/Blazy.Sockets/blob/master/docs/assets/server1.png)

## Small sample of using this framework:

#### 1. First, create packet handler with request dto
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


#### 2. Register packet handler in Program.cs
```csharp
var builder = new NetworkBuilder();

builder.Use<IAuthService, AuthService>();
builder.UsePacketHandler<AuthHandler>("Auth");

using var app = builder.Build();
var server = app.Resolve<INetworkServer>();
await server.RunAsync();
```

#### 3. Send packet with this event name. And that`s it!
```csharp
var builder = new NetworkBuilder();
using var app = builder.Build();
var client = app.Resolve<INetworkClient>();

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
- Middlewares
- Refactor PacketHandler
- Command Parser
