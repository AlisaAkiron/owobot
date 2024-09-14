using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using owoBot.Module.ExchangeRateApi.Services;

namespace owoBot.Module.ExchangeRateApi;

public static class Extension
{
    public static void AddExchangeRateApi(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<ExchangeRateApiClient>();
    }
}
