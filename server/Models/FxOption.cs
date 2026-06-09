namespace FxOptions.Server.Models;

public record FxOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime UpdatedAt { get; set; }
}
