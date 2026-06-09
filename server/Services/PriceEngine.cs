using FxOptions.Server.Models;

namespace FxOptions.Server.Services;

public interface IPriceEngine
{
    IReadOnlyList<PriceDelta> GenerateTick();
}

public class PriceEngine : IPriceEngine
{
    private readonly IOptionsRepository _repository;
    private readonly Random _random;

    public PriceEngine(IOptionsRepository repository) : this(repository, new Random()) { }

    /// <summary>
    /// Test-friendly constructor — accepts a seeded Random for deterministic price generation.
    /// </summary>
    public PriceEngine(IOptionsRepository repository, Random random)
    {
        _repository = repository;
        _random = random;
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
