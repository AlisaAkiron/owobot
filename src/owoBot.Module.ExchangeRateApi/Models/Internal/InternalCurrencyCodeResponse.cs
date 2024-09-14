using System.Text.Json.Serialization;

namespace owoBot.Module.ExchangeRateApi.Models.Internal;

internal sealed record InternalCurrencyCodeResponse
{
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("supported_codes")]
    public List<string[]> Codes { get; set; } = [];
}
