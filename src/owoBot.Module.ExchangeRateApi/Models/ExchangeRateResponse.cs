using owoBot.Domain.Types;

namespace owoBot.Module.ExchangeRateApi.Models;

public class ExchangeRateResponse
{
    public DateTimeOffset LastUpdateTime { get; init; }

    public DateTimeOffset NextUpdateTime { get; init; }

    public CurrencyCode BaseCode { get; init; }

    public Dictionary<CurrencyCode, decimal> ExchangeRates { get; init; } = [];
}
