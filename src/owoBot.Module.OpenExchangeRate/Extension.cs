using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using owoBot.Module.OpenExchangeRate.Services;

namespace owoBot.Module.OpenExchangeRate;

public static class Extension
{
    public static void AddOpenExchangeRateModule(IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<OpenExchangeRateClient>();
        builder.Services.AddTransient<ExchangeRateManager>();
    }
}
