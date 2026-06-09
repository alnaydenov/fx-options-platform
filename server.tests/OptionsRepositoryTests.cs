using FxOptions.Server.Models;
using FxOptions.Server.Services;

namespace FxOptions.Server.Tests;

public class OptionsRepositoryTests
{
    private static List<FxOption> CreateSeedData() => new()
    {
        new FxOption { Id = 1, Name = "EUR/USD Call", Price = 0.0234m, UpdatedAt = DateTime.UtcNow },
        new FxOption { Id = 2, Name = "GBP/USD Put", Price = 0.0156m, UpdatedAt = DateTime.UtcNow },
    };

    [Fact]
    public void GetAll_ReturnsSeedData()
    {
        var seed = CreateSeedData();
        var repo = new OptionsRepository(seed);

        var result = repo.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal("EUR/USD Call", result[0].Name);
        Assert.Equal(0.0234m, result[0].Price);
    }

    [Fact]
    public void GetAll_ReturnsDefensiveCopy()
    {
        var seed = CreateSeedData();
        var repo = new OptionsRepository(seed);

        var first = repo.GetAll();
        var second = repo.GetAll();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void UpdatePrices_AppliesDeltasCorrectly()
    {
        var seed = CreateSeedData();
        var repo = new OptionsRepository(seed);
        var now = DateTime.UtcNow;

        var deltas = new List<PriceDelta>
        {
            new(1, 0.0250m, now),
        };

        repo.UpdatePrices(deltas);

        var result = repo.GetAll();
        Assert.Equal(0.0250m, result[0].Price);
        Assert.Equal(now, result[0].UpdatedAt);
        // Item 2 should be unchanged
        Assert.Equal(0.0156m, result[1].Price);
    }

    [Fact]
    public void UpdatePrices_IgnoresUnknownIds()
    {
        var seed = CreateSeedData();
        var repo = new OptionsRepository(seed);

        var deltas = new List<PriceDelta>
        {
            new(999, 1.0000m, DateTime.UtcNow),
        };

        repo.UpdatePrices(deltas);

        var result = repo.GetAll();
        Assert.Equal(2, result.Count);
        Assert.Equal(0.0234m, result[0].Price);
    }
}
