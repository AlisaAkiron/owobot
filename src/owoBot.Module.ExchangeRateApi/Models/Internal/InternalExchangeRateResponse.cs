using System.Text.Json.Serialization;

namespace owoBot.Module.ExchangeRateApi.Models.Internal;

public record InternalExchangeRateResponse
{
    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;

    [JsonPropertyName("time_last_update_utc")]
    public DateTimeOffset LastUpdateTime { get; init; }

    [JsonPropertyName("time_next_update_utc")]
    public DateTimeOffset NextUpdateTime { get; init; }

    [JsonPropertyName("base_code")]
    public string BaseCode { get; init; } = string.Empty;

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> Rates { get; init; } = new();
}
