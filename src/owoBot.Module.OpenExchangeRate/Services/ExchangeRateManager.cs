using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    public async Task<ExchangeResult> ExchangeAsync(decimal sourceAmount, string source, string target)
    {
        // If source and target are the same, return the amount
        if (source == target)
        {
            return ExchangeResult.Success(sourceAmount, source, sourceAmount, target);
        }

        // Find source -> target
        var directExchangeRate = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == source && x.Target == target);
        if (directExchangeRate is not null)
        {
            var directTargetAmount = sourceAmount * directExchangeRate.Rate;
            return ExchangeResult.Success(sourceAmount, source, directTargetAmount, target);
        }

        // Find target -> source
        var inverseExchangeRate = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == target && x.Target == source);
        if (inverseExchangeRate is not null)
        {
            var inverseTargetAmount = sourceAmount / inverseExchangeRate.Rate;
            return ExchangeResult.Success(sourceAmount, source, inverseTargetAmount, target);
        }

        // Get from API, update database, Find source -> target
        var directCurrencyExchangeRate = await _openExchangeRateClient.GetCurrencyExchangeRate(source);
        await _dbContext.UpdateExchangeRateAsync(directCurrencyExchangeRate);

        var hasValue = directCurrencyExchangeRate.Rates.TryGetValue(target, out var er);
        if (hasValue)
        {
            var directTargetAmount = sourceAmount * er;
            return ExchangeResult.Success(sourceAmount, source, directTargetAmount, target);
        }

        // if any of currency is USD, abort
        if (source == "USD" || target == "USD")
        {
            return ExchangeResult.Fail(sourceAmount, source, target, "USD is not found in the exchange rate database.");
        }

        // Find source -{inverse}-> USD -{direct}-> target
        var usdToSource = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == "USD" && x.Target == source);
        var usdToTarget = await _dbContext
            .ExchangeRates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Source == "USD" && x.Target == target);

        if (usdToSource is null || usdToTarget is null)
        {
            return ExchangeResult.Fail(sourceAmount, source, target, "USD is not found in the exchange rate database.");
        }

        var usdAmount = sourceAmount / usdToSource.Rate;
        var targetAmount = usdAmount * usdToTarget.Rate;

        return ExchangeResult.Success(sourceAmount, source, targetAmount, target, [
            new IntermediaExchangeResult(usdAmount, "USD")
        ]);
    }
}
