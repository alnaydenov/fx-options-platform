using FxOptions.Server.Models;
using FxOptions.Server.Services;
using NSubstitute;

namespace FxOptions.Server.Tests;

public class PriceEngineTests
{
    private static IOptionsRepository CreateMockRepo(List<FxOption> items)
    {
        var repo = Substitute.For<IOptionsRepository>();
        repo.GetAll().Returns(items.AsReadOnly());
        return repo;
    }

    [Fact]
    public void GenerateTick_ReturnsDeltasForChangedItems()
    {
        var items = new List<FxOption>
        {
            new() { Id = 1, Name = "Test", Price = 1.0000m, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Test2", Price = 2.0000m, UpdatedAt = DateTime.UtcNow },
        };
        var repo = CreateMockRepo(items);

        // Seed Random so results are deterministic
        var engine = new PriceEngine(repo, new Random(42));

        var deltas = engine.GenerateTick();

        // With seed 42 and 70% probability, we should get at least some deltas
        Assert.NotEmpty(deltas);
        Assert.All(deltas, d => Assert.True(d.Price > 0));
    }

    [Fact]
    public void GenerateTick_UpdatesRepositoryWithDeltas()
    {
        var items = new List<FxOption>
        {
            new() { Id = 1, Name = "Test", Price = 1.0000m, UpdatedAt = DateTime.UtcNow },
        };
        var repo = CreateMockRepo(items);
        var engine = new PriceEngine(repo, new Random(42));

        var deltas = engine.GenerateTick();

        repo.Received().UpdatePrices(Arg.Any<IEnumerable<PriceDelta>>());
    }

    [Fact]
    public void GenerateTick_PricesNeverGoNegative()
    {
        var items = new List<FxOption>
        {
            new() { Id = 1, Name = "Tiny", Price = 0.0001m, UpdatedAt = DateTime.UtcNow },
        };
        var repo = CreateMockRepo(items);
        var engine = new PriceEngine(repo, new Random(123));

        // Run many ticks to stress-test the floor
        for (int i = 0; i < 100; i++)
        {
            var deltas = engine.GenerateTick();
            Assert.All(deltas, d => Assert.True(d.Price > 0));
        }
    }

    [Fact]
    public void GenerateTick_WithNoItems_ReturnsEmptyList()
    {
        var repo = CreateMockRepo(new List<FxOption>());
        var engine = new PriceEngine(repo, new Random(1));

        var deltas = engine.GenerateTick();

        Assert.Empty(deltas);
    }
}
