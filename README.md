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

#### 0. Setup your configuration file in appsettings.json for server
```json
{
  "Host": {
    "IPAddress": "0.0.0.0",
    "Port": 8080
  }
}
```

#### And for client also (if the client and server are in the different projects)
```json
{
  "Connection": {
    "IPAddress": "127.0.0.1",
    "RemotePort": 8080
  }
}
```

#### 1. First, create packet handler with request dto (implement your service for auth and inject it)
```csharp
public class AuthRequest
{
    public string Username { get; init; } = default!;
}

public class AuthHandler : PacketHandler<AuthRequest>
{
    private readonly IAuthService _authService;

    public AuthHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task HandleAsync(AuthRequest request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Username
        };

        var authenticated = _authService.Authenticate(user);

        if (!authenticated)
        {
            await SendResponseAsync(new Packet
            {
                Event = "Username Is Taken",
                State = new 
                {
                    Username = user.Name
                }
            }, ct);
            return;
        }
        
        await SendResponseAsync(new Packet
        {
            Event = "Authenticated",
            State = new 
            {
                Username = user.Name
            }
        }, ct);
    }
}
```

#### 2. Create Middleware with logging
```csharp
public class MetricsMiddleware : MiddlewareBase
{
    private readonly ILogger _logger;

    public MetricsMiddleware(ILogger logger)
    {
        _logger = logger;
    }
    
    public override async Task InvokeAsync(Packet request, INetworkClient client, PacketDelegate next, CancellationToken ct = default)
    {
        var startDate = DateTime.UtcNow;
        
        await next(request, client, ct);
        
        var endDate = DateTime.UtcNow;
        var elapsed = (endDate - startDate).TotalMilliseconds;
        
        _logger.Information("Packet for {Event} sent by {RemoteEndPoint} was handled in {ElapsedMilliseconds}ms", request.Event, client.RemoteEndPoint, elapsed);
    }
}
```

#### 3. Register packet handler and middleware in Program.cs
```csharp
var builder = new NetworkBuilder();

builder.UseDefaultLogger();
builder.UseDefaultEncoder();
builder.Use<IAuthService, AuthService>();
builder.UsePacketHandler<AuthHandler>("Auth");
builder.UseMiddleware<MetricsMiddleware>();

using var app = builder.Build();
var server = app.Resolve<INetworkServer>();
await server.RunAsync();
```

#### 4. Send packet with this event name (you can build client in another project)
```csharp
var builder = new NetworkBuilder();

builder.UseDefaultLogger();
builder.UseDefaultEncoder();

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
- Command Parser
