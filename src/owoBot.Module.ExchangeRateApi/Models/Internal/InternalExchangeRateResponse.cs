using System.Text.Json.Serialization;
using owoBot.Domain.Converters;

namespace owoBot.Module.ExchangeRateApi.Models.Internal;

internal sealed record InternalExchangeRateResponse
{
    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;

    [JsonPropertyName("time_last_update_unix")]
    [JsonConverter(typeof(UnixTimestampDataTimeOffsetJsonConverter))]
    public DateTimeOffset LastUpdateTime { get; init; }

    [JsonPropertyName("time_next_update_unix")]
    [JsonConverter(typeof(UnixTimestampDataTimeOffsetJsonConverter))]
    public DateTimeOffset NextUpdateTime { get; init; }

    [JsonPropertyName("base_code")]
    public string BaseCode { get; init; } = string.Empty;

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> Rates { get; set; } = [];
}
