using System.Text.Json;
using FxOptions.Server.Models;

namespace FxOptions.Server.Services;

public interface IOptionsRepository
{
    IReadOnlyList<FxOption> GetAll();
    void UpdatePrices(IEnumerable<PriceDelta> deltas);
}

public class OptionsRepository : IOptionsRepository
{
    private readonly List<FxOption> _items;
    private readonly object _lock = new();

    public OptionsRepository()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "items.json");
        var json = File.ReadAllText(jsonPath);
        _items = JsonSerializer.Deserialize<List<FxOption>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<FxOption>();
    }

    public IReadOnlyList<FxOption> GetAll()
    {
        lock (_lock)
        {
            return _items.ToList().AsReadOnly();
        }
    }

    public void UpdatePrices(IEnumerable<PriceDelta> deltas)
    {
        lock (_lock)
        {
            foreach (var delta in deltas)
            {
                var item = _items.FirstOrDefault(x => x.Id == delta.Id);
                if (item != null)
                {
                    item.Price = delta.Price;
                    item.UpdatedAt = delta.UpdatedAt;
                }
            }
        }
    }
}
