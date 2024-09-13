using System.Net;
using Discord;
using Discord.Interactions;
using Discord.Net.WebSockets;
using Discord.Rest;
using Discord.WebSocket;
using owoBot.App.Bot.Discord;
using owoBot.App.Bot.Services;
using owoBot.Application.Command;
using owoBot.Domain.Constants;
using owoBot.EntityFrameworkCore;

namespace owoBot.App.Bot;

public static class Extension
{
    public static WebApplicationBuilder AddBotServices(this WebApplicationBuilder builder)
    {
        builder.AddDiscordBot();
        builder.AddHttpClients();

        builder.AddApplicationCommandServices();
        builder.AddEntityFrameworkCore();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracer =>
            {
                tracer.AddSource(ActivitySources.AllActivitySources.ToArray());
            });

        return builder;
    }

    private static void AddDiscordBot(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var networkProxyEnabled = configuration.GetValue("Network:Proxy:Enabled", false);
        var networkProxyHost = configuration.GetValue("Network:Proxy:Host", string.Empty);
        var webProxy = networkProxyEnabled ? new WebProxy(networkProxyHost) : null;

        builder.Services.AddSingleton<DiscordSocketConfig>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new DiscordSocketConfig
            {
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.None,
                WebSocketProvider = DefaultWebSocketProvider.Create(webProxy),
                RestClientProvider = url => new DiscordHttpRestClient(url, httpClientFactory)
            };
        });

        builder.Services.AddSingleton<DiscordSocketClient>();

        builder.Services.AddSingleton<DiscordRestConfig>(sp => sp.GetRequiredService<DiscordSocketConfig>());
        builder.Services.AddSingleton<DiscordRestClient>();

        builder.Services.AddSingleton<InteractionServiceConfig>(_ => new InteractionServiceConfig
        {
            UseCompiledLambda = true
        });
        builder.Services.AddSingleton<InteractionService>();

        builder.Services.AddHostedService<DiscordHostedService>();
    }

    private static void AddHttpClients(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var networkProxyEnabled = configuration.GetValue("Network:Proxy:Enabled", false);
        var networkProxyHost = configuration.GetValue("Network:Proxy:Host", string.Empty);
        var webProxy = networkProxyEnabled ? new WebProxy(networkProxyHost) : null;

        builder.Services.AddHttpClient("DiscordRest", client =>
            {
                client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
                Proxy = webProxy,
                UseProxy = networkProxyEnabled
            });
    }
}
