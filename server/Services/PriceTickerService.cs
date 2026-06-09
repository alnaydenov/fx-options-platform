using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FxOptions.Server.Models;

namespace FxOptions.Server.Services;

public class PriceTickerService : BackgroundService
{
    private const int TickIntervalMs = 1000; // 1 second tick interval

    private readonly IPriceEngine _priceEngine;
    private readonly ISubscriberManager _subscribers;
    private readonly ILogger<PriceTickerService> _logger;

    public PriceTickerService(
        IPriceEngine priceEngine,
        ISubscriberManager subscribers,
        ILogger<PriceTickerService> logger)
    {
        _priceEngine = priceEngine;
        _subscribers = subscribers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceTickerService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TickIntervalMs, stoppingToken);

            if (!_subscribers.HasSubscribers) continue;

            var deltas = _priceEngine.GenerateTick();
            if (deltas.Count == 0) continue;

            // Minimal wire format: array of [id, price, timestamp] tuples
            var payload = SerializeDeltas(deltas);
            var buffer = Encoding.UTF8.GetBytes(payload);
            var segment = new ArraySegment<byte>(buffer);

            var deadSockets = new List<string>();

            foreach (var (id, socket) in _subscribers.GetAll())
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(segment, WebSocketMessageType.Text, true, stoppingToken);
                    }
                    catch
                    {
                        deadSockets.Add(id);
                    }
                }
                else
                {
                    deadSockets.Add(id);
                }
            }

            foreach (var id in deadSockets)
            {
                _subscribers.Remove(id);
            }
        }
    }

    private static string SerializeDeltas(IReadOnlyList<PriceDelta> deltas)
    {
        // Wire format: [[id, price, "timestamp"], ...]
        var tuples = deltas.Select(d => new object[]
        {
            d.Id,
            d.Price,
            d.UpdatedAt.ToString("O")
        });

        return JsonSerializer.Serialize(tuples);
    }
}
