using System.Text.Json.Serialization;

namespace owoBot.Module.OpenExchangeRate.Models.Internal;

internal record InternalExchangeRate
{
    [JsonPropertyName("disclaimer")]
    public string Disclaimer { get; set; } = string.Empty;

    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = [];
}
