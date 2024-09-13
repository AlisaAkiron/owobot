namespace owoBot.Module.OpenExchangeRate.Models;

public record CurrencyExchangeRate
{
    public string Base { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }

    public Dictionary<string, decimal> Rates { get; set; } = [];
}
