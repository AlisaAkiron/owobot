using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using owoBot.Module.OpenExchangeRate.Services;

namespace owoBot.Module.OpenExchangeRate;

public static class Extension
{
    public static void AddOpenExchangeRateModule(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<OpenExchangeRateClient>();
        builder.Services.TryAddScoped<ExchangeRateManager>();
    }
}
