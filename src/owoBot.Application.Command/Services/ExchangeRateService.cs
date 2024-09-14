using Microsoft.EntityFrameworkCore;
using owoBot.Domain.Entities;
using owoBot.Domain.Types;
using owoBot.EntityFrameworkCore;
using owoBot.EntityFrameworkCore.Extensions;
using owoBot.Module.ExchangeRateApi.Services;

namespace owoBot.Application.Command.Services;

[AutoConstructor]
public partial class ExchangeRateService
{
    private readonly ExchangeRateApiClient _exchangeRateApiClient;
    private readonly TimeProvider _timeProvider;
    private readonly OWODbContext _dbContext;

    public async Task<List<CurrencyCode>> GetSupportedCurrenciesAsync(string? codeSearchPattern = null, string? nameSearchPattern = null)
    {
        var now = _timeProvider.GetUtcNow();

        var lastUpdate = await _dbContext.Caches.GetStringAsync("ExchangeRateApi:SupportedCurrencies:LastUpdate");
        var isValid = DateTimeOffset.TryParse(lastUpdate, out var lastUpdateTime);

        if (isValid is false || lastUpdateTime.AddMonths(1) < now)
        {
            await UpdateSupportedCurrencies(now);
        }

        var supportedCurrenciesQueryable = _dbContext.CurrencyInfos.AsQueryable();
        if (string.IsNullOrEmpty(codeSearchPattern) is false)
        {
            supportedCurrenciesQueryable = supportedCurrenciesQueryable.Where(x => EF.Functions.ILike(x.Code, codeSearchPattern));
        }

        if (string.IsNullOrEmpty(nameSearchPattern) is false)
        {
            supportedCurrenciesQueryable = supportedCurrenciesQueryable.Where(x => EF.Functions.ILike(x.Name, nameSearchPattern));
        }

        var supportedCurrencies = await supportedCurrenciesQueryable.ToListAsync();

        return supportedCurrencies
            .Select(x => CurrencyCode.From(x.Code, x.Name))
            .ToList();
    }

    public async Task<(ExchangeRate, DateTimeOffset)?> DirectExchangeAsync(CurrencyCode sourceCurrency, CurrencyCode targetCurrency)
    {
        var now = _timeProvider.GetUtcNow();

        var nextUpdate = await _dbContext.Caches.GetStringAsync($"ExchangeRateApi:ExchangeRates:{sourceCurrency}:NextUpdate");
        var isNextUpdateValid = DateTimeOffset.TryParse(nextUpdate, out var nextUpdateTime);

        if (isNextUpdateValid is false || nextUpdateTime < now)
        {
            await UpdateExchangeRateAsync(sourceCurrency);
        }

        var exchangeRate = await _dbContext.ExchangeRates
            .AsNoTracking()
            .Where(x => x.Source == sourceCurrency && x.Target == targetCurrency)
            .FirstOrDefaultAsync();

        if (exchangeRate is null)
        {
            return null;
        }

        var lastUpdate = await _dbContext.Caches.GetStringAsync($"ExchangeRateApi:ExchangeRates:{sourceCurrency}:LastUpdate");
        var lastUpdateTime = DateTimeOffset.Parse(lastUpdate);

        return (exchangeRate, lastUpdateTime);
    }

    private async Task UpdateSupportedCurrencies(DateTimeOffset now)
    {
        var currenciesFromApi = await _exchangeRateApiClient.GetSupportedCurrenciesAsync();
        var currencies = currenciesFromApi
            .Select(x => new CurrencyInfo
            {
                Code = x.Key,
                Name = x.Value
            })
            .ToList();

        await _dbContext.CurrencyInfos.ExecuteDeleteAsync();

        await _dbContext.Caches.SetStringAsync("ExchangeRateApi:SupportedCurrencies:LastUpdate", now.ToString());
        await _dbContext.CurrencyInfos.AddRangeAsync(currencies);
        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateExchangeRateAsync(CurrencyCode sourceCurrency)
    {
        var exchangeRateFromApi = await _exchangeRateApiClient.GetExchangeRateAsync(sourceCurrency);
        var exchangeRates = exchangeRateFromApi.ExchangeRates
            .Select(x => new ExchangeRate
            {
                Source = sourceCurrency,
                Target = x.Key,
                Rate = x.Value
            })
            .ToList();

        await _dbContext.ExchangeRates.Where(x => x.Source == sourceCurrency).ExecuteDeleteAsync();

        await _dbContext.Caches.SetStringAsync($"ExchangeRateApi:ExchangeRates:{sourceCurrency}:LastUpdate", exchangeRateFromApi.LastUpdateTime.ToString());
        await _dbContext.Caches.SetStringAsync($"ExchangeRateApi:ExchangeRates:{sourceCurrency}:NextUpdate", exchangeRateFromApi.NextUpdateTime.ToString());
        await _dbContext.ExchangeRates.AddRangeAsync(exchangeRates);
        await _dbContext.SaveChangesAsync();
    }
}
