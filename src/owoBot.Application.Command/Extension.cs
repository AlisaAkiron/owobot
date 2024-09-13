using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using owoBot.Domain.Abstract;

namespace owoBot.Application.Command;

public static class Extension
{
    public static void AddApplicationCommandServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDiscordApplicationInitializer, CommandInitializer>();
    }
}
