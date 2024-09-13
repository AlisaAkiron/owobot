using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using owoBot.Domain.Abstract;
using owoBot.Module.OpenExchangeRate;

namespace owoBot.Application.Command;

public static class Extension
{
    public static void AddApplicationCommandServices(this IHostApplicationBuilder builder)
    {
        builder.AddOpenExchangeRateModule();

        builder.Services.AddSingleton<IDiscordApplicationInitializer, CommandInitializer>();
    }
}
