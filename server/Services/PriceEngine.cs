using FxOptions.Server.Models;

namespace FxOptions.Server.Services;

public interface IPriceEngine
{
    IReadOnlyList<PriceDelta> GenerateTick();
}

public class PriceEngine : IPriceEngine
{
    private readonly IOptionsRepository _repository;
    private readonly Random _random = new();

    public PriceEngine(IOptionsRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<PriceDelta> GenerateTick()
    {
        var items = _repository.GetAll();
        var deltas = new List<PriceDelta>();
        var now = DateTime.UtcNow;

        foreach (var item in items)
        {
            // ~70% chance of price change per tick
            if (_random.NextDouble() < 0.7)
            {
                var changePercent = (_random.NextDouble() - 0.5) * 0.02; // ±1%
                var newPrice = Math.Round(item.Price * (1 + (decimal)changePercent), 4);
                if (newPrice <= 0) newPrice = 0.0001m;
                deltas.Add(new PriceDelta(item.Id, newPrice, now));
            }
        }

        _repository.UpdatePrices(deltas);
        return deltas.AsReadOnly();
    }
}
