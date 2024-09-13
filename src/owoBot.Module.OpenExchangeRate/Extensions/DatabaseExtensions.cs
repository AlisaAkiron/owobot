using Microsoft.EntityFrameworkCore;
using owoBot.Domain.Entities;
using owoBot.EntityFrameworkCore;
using owoBot.Module.OpenExchangeRate.Models;

namespace owoBot.Module.OpenExchangeRate.Extensions;

public static class DatabaseExtensions
{
    public static async Task UpdateExchangeRateAsync(this OWODbContext dbContext, CurrencyExchangeRate currencyExchangeRate)
    {
        var existingExchangeRate = await dbContext.ExchangeRates
            .Where(x => x.Source == currencyExchangeRate.Base)
            .ToListAsync();

        foreach (var exchangeRate in existingExchangeRate)
        {
            var hasValue = currencyExchangeRate.Rates.TryGetValue(exchangeRate.Target, out var targetExchangeRate);

            if (hasValue)
            {
                exchangeRate.Rate = targetExchangeRate;
                exchangeRate.LastUpdated = currencyExchangeRate.Timestamp;

                dbContext.ExchangeRates.Update(exchangeRate);
                continue;
            }

            dbContext.ExchangeRates.Remove(exchangeRate);
        }

        var newExchangeRate = currencyExchangeRate.Rates
            .Where(x => existingExchangeRate.All(y => y.Target != x.Key))
            .Select(x => new ExchangeRate
            {
                Source = currencyExchangeRate.Base,
                Target = x.Key,
                Rate = x.Value,
                LastUpdated = currencyExchangeRate.Timestamp
            });

        await dbContext.ExchangeRates.AddRangeAsync(newExchangeRate);

        await dbContext.SaveChangesAsync();
    }
}
