using System.Text.Json.Serialization;
using owoBot.Module.OpenExchangeRate.Converters;

namespace owoBot.Module.OpenExchangeRate.Models.Internal;

internal sealed record InternalExchangeRate
{
    [JsonPropertyName("disclaimer")]
    public string Disclaimer { get; set; } = string.Empty;

    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(UnixTimestampToDateTimeOffsetConverter))]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal> Rates { get; set; } = [];
}
