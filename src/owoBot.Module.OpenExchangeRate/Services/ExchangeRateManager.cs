using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using owoBot.Domain.Entities;
using owoBot.EntityFrameworkCore;
using owoBot.Module.OpenExchangeRate.Extensions;
using owoBot.Module.OpenExchangeRate.Models;

namespace owoBot.Module.OpenExchangeRate.Services;

[AutoConstructor]
public partial class ExchangeRateManager
{
    private readonly OpenExchangeRateClient _openExchangeRateClient;
    private readonly IConfiguration _configuration;
    private readonly OWODbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public async Task<List<CurrencyInfo>> GetCurrencies(string? codePattern = null, string? namePattern = null)
    {
        var currenciesForceUpdateInterval = GetTimeSpan("Modules:OpenExchangeRate:ForceUpdate:Currencies", 720);
        var now = _timeProvider.GetUtcNow();

        var firstValue = await _dbContext.CurrencyInfos.FirstOrDefaultAsync();
        if (firstValue is null || firstValue.LastUpdated.Add(currenciesForceUpdateInterval) < now)
        {
            var currencies = await _openExchangeRateClient.GetCurrencies();
            var infos = currencies
                .Select(x => new CurrencyInfo
                {
                    Code = x.Code,
                    Name = x.Name,
                    LastUpdated = now
                })
                .ToList();
            await _dbContext.UpdateCurrencyInfoAsync(infos);
        }

        var queryable = _dbContext.CurrencyInfos.AsNoTracking();
        if (codePattern is not null)
        {
            queryable = queryable.Where(x => EF.Functions.ILike(x.Code, codePattern));
        }
        if (namePattern is not null)
        {
            queryable = queryable.Where(x => EF.Functions.ILike(x.Name, namePattern));
        }

        var currencyInfos = await queryable
            .OrderBy(x => x.Code)
            .ToListAsync();

        return currencyInfos;
    }

    // 1. IF source == target, return sourceAmount
    // 2. IF source -> target exists and valid, return sourceAmount * rate
    // 3. IF target -> source exists and valid, return sourceAmount / rate
    // 4. Update source -> target, try 2 again
    // 5. IF source or target is USD, abort
    // 6. IF source -> USD or USD -> target not exists, abort

    public async Task<List<ExchangeResult>> ExchangeAsync(decimal sourceAmount, string source, string target)
    {
        // If source and target are the same, return the amount
        if (source == target)
        {
            return [ExchangeResult.Success(sourceAmount, source, sourceAmount, target, 1, _timeProvider.GetUtcNow())];
        }

        var currenciesForceUpdateInterval = GetTimeSpan("Modules:OpenExchangeRate:ForceUpdate:ExchangeRates", 1);
        var now = _timeProvider.GetUtcNow();

        var plan = _configuration["Modules:OpenExchangeRate:Plan"] ?? "Free";

        // Find source -> target
        if (CheckDirectExchangeEligibility(source, target, plan))
        {
            var directExchangeRate = await _dbContext
                .ExchangeRates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == source && x.Target == target);
            if (directExchangeRate is not null && directExchangeRate.LastUpdated.Add(currenciesForceUpdateInterval) > now)
            {
                var directTargetAmount = sourceAmount * directExchangeRate.Rate;
                return [ExchangeResult.Success(sourceAmount, source, directTargetAmount, target, directExchangeRate.Rate, directExchangeRate.LastUpdated)];
            }
        }

        // Find target -> source
        if (CheckDirectExchangeEligibility(target, source, plan))
        {
            var inverseExchangeRate = await _dbContext
                .ExchangeRates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == target && x.Target == source);
            if (inverseExchangeRate is not null && inverseExchangeRate.LastUpdated.Add(currenciesForceUpdateInterval) > now)
            {
                var inverseTargetAmount = sourceAmount / inverseExchangeRate.Rate;
                return [ExchangeResult.Success(sourceAmount, source, inverseTargetAmount, target, 1 / inverseExchangeRate.Rate, inverseExchangeRate.LastUpdated)];
            }
        }

        // Get from API, update database, Find source -> target
        if (CheckDirectExchangeEligibility(source, target, plan))
        {
            var directCurrencyExchangeRate = await _openExchangeRateClient.GetCurrencyExchangeRate(source);
            await _dbContext.UpdateExchangeRateAsync(directCurrencyExchangeRate);

            var hasValue = directCurrencyExchangeRate.Rates.TryGetValue(target, out var er);
            if (hasValue)
            {
                var directTargetAmount = sourceAmount * er;
                return [ExchangeResult.Success(sourceAmount, source, directTargetAmount, target, er, directCurrencyExchangeRate.Timestamp)];
            }
        }

        // if any of currency is USD, abort
        if (source == "USD" || target == "USD")
        {
            return[ExchangeResult.Fail(sourceAmount, source, target, "USD is not found in the exchange rate database.")];
        }

        // Find source -{inverse}-> USD -{direct}-> target
        var usdSource = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == "USD" && x.Target == source);
        var usdTarget = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == "USD" && x.Target == target);

        if (usdSource is not null && usdTarget is not null &&
            usdSource.LastUpdated.Add(currenciesForceUpdateInterval) > now &&
            usdTarget.LastUpdated.Add(currenciesForceUpdateInterval) > now)
        {
            var usdAmount = sourceAmount / usdSource.Rate;
            var targetAmount = usdAmount * usdTarget.Rate;

            return [
                ExchangeResult.Success(sourceAmount, source, usdAmount, "USD", 1 / usdSource.Rate, usdSource.LastUpdated),
                ExchangeResult.Success(usdAmount, "USD", targetAmount, target, usdTarget.Rate, usdTarget.LastUpdated)
            ];
        }

        var usdCurrencyExchangeRate = await _openExchangeRateClient.GetCurrencyExchangeRate();
        await _dbContext.UpdateExchangeRateAsync(usdCurrencyExchangeRate);

        if (usdCurrencyExchangeRate.Rates.TryGetValue(source, out var usdSourceRate) &&
            usdCurrencyExchangeRate.Rates.TryGetValue(target, out var usdTargetRate))
        {
            var usdAmount = sourceAmount / usdSourceRate;
            var targetAmount = usdAmount * usdTargetRate;

            return [
                ExchangeResult.Success(sourceAmount, source, usdAmount, "USD", 1 / usdSourceRate, usdCurrencyExchangeRate.Timestamp),
                ExchangeResult.Success(usdAmount, "USD", targetAmount, target, usdTargetRate, usdCurrencyExchangeRate.Timestamp)
            ];
        }

        return [ExchangeResult.Fail(sourceAmount, source, target, "Cannot find suitable exchange path.")];
    }

    private TimeSpan GetTimeSpan(string key, int defaultValue)
    {
        var value = _configuration[key];
        var success = int.TryParse(value, out var result);
        return TimeSpan.FromHours(success ? result : defaultValue);
    }

    private bool CheckDirectExchangeEligibility(string source, string target, string plan)
    {
        if (source == "USD" || target == "USD")
        {
            return true;
        }

        return plan != "Free";
    }
}
