using FxOptions.Server.Models;
using FxOptions.Server.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FxOptions.Server.Tests;

public class PriceTickerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_DoesNotTickWhenNoSubscribers()
    {
        var priceEngine = Substitute.For<IPriceEngine>();
        var subscribers = Substitute.For<ISubscriberManager>();
        subscribers.HasSubscribers.Returns(false);
        var logger = Substitute.For<ILogger<PriceTickerService>>();

        var service = new PriceTickerService(priceEngine, subscribers, logger);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500));
        await service.StartAsync(cts.Token);
        await Task.Delay(1200); // allow at least one tick cycle
        await service.StopAsync(CancellationToken.None);

        priceEngine.DidNotReceive().GenerateTick();
    }

    [Fact]
    public async Task ExecuteAsync_CallsGenerateTickWhenSubscribersExist()
    {
        var priceEngine = Substitute.For<IPriceEngine>();
        priceEngine.GenerateTick().Returns(new List<PriceDelta>().AsReadOnly());
        var subscribers = Substitute.For<ISubscriberManager>();
        subscribers.HasSubscribers.Returns(true);
        subscribers.GetAll().Returns(Array.Empty<KeyValuePair<string, System.Net.WebSockets.WebSocket>>());
        var logger = Substitute.For<ILogger<PriceTickerService>>();

        var service = new PriceTickerService(priceEngine, subscribers, logger);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500));
        await service.StartAsync(cts.Token);
        await Task.Delay(1200); // allow at least one tick
        await service.StopAsync(CancellationToken.None);

        priceEngine.Received().GenerateTick();
    }
}
