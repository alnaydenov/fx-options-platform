using System.Net.WebSockets;
using FxOptions.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOptionsRepository, OptionsRepository>();
builder.Services.AddSingleton<IPriceEngine, PriceEngine>();
builder.Services.AddSingleton<ISubscriberManager, SubscriberManager>();
builder.Services.AddHostedService<PriceTickerService>();

var app = builder.Build();

app.UseWebSockets();

app.MapGet("/api/options", (IOptionsRepository repo) => repo.GetAll());

app.Map("/ws", async (HttpContext context, ISubscriberManager subscribers, ILogger<Program> logger) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();
    var subscriberId = Guid.NewGuid().ToString();

    subscribers.Add(subscriberId, socket);
    logger.LogInformation("Client {Id} subscribed", subscriberId);

    var buffer = new byte[256];
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }
    }
    catch (WebSocketException) { }
    finally
    {
        subscribers.Remove(subscriberId);
        logger.LogInformation("Client {Id} unsubscribed", subscriberId);

        if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
        }
    }
});

app.Run();
