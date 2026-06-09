namespace FxOptions.Server.Models;

/// <summary>
/// Minimal wire format: [id, price, timestamp] as a tuple array
/// </summary>
public record PriceDelta(int Id, decimal Price, DateTime UpdatedAt);
