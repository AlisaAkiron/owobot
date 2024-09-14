using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using owoBot.Application.Command.Services;
using owoBot.Domain.Abstract;
using owoBot.Module.ExchangeRateApi;

namespace owoBot.Application.Command;

public static class Extension
{
    public static void AddApplicationCommandServices(this IHostApplicationBuilder builder)
    {
        builder.AddExchangeRateApi();

        builder.Services.AddScoped<ExchangeRateService>();

        builder.Services.AddSingleton<IDiscordApplicationInitializer, CommandInitializer>();
    }
}
