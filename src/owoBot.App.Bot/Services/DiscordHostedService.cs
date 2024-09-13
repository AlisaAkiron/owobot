using Discord;
using Discord.Rest;
using Discord.WebSocket;
using owoBot.App.Bot.Extensions;
using owoBot.Domain.Abstract;
using owoBot.Domain.Constants;

namespace owoBot.App.Bot.Services;

[AutoConstructor]
public partial class DiscordHostedService : IHostedService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly DiscordRestClient _discordRestClient;
    private readonly ILogger<DiscordHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<IDiscordApplicationInitializer> _applicationInitializers;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // ReSharper disable once ExplicitCallerInfoArgument
        var initializeActivity = ActivitySources.AppActivitySource.StartActivity("Initialize");

        RegisterDiscordRestClientEvents();
        RegisterDiscordSocketClientEvents();

        var botToken = _configuration["Discord:BotToken"];

        await _discordSocketClient.LoginAsync(TokenType.Bot, botToken);
        await _discordRestClient.LoginAsync(TokenType.Bot, botToken);

        initializeActivity?.Stop();
        initializeActivity?.Dispose();

        await _discordSocketClient.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordSocketClient.StopAsync();
    }

    private void RegisterDiscordSocketClientEvents()
    {
        _discordSocketClient.Log += WriteDiscordLog;
        foreach (var initializer in _applicationInitializers)
        {
            initializer.SocketInitializer(_discordSocketClient);
        }
    }

    private void RegisterDiscordRestClientEvents()
    {
        _discordRestClient.Log += WriteDiscordLog;
        foreach (var initializer in _applicationInitializers)
        {
            initializer.RestInitializer(_discordRestClient);
        }
    }

    private Task WriteDiscordLog(LogMessage msg)
    {
        _logger.Log(msg.Severity.MapToLogLevel(), msg.Exception,
            "{Source} - {Message}", msg.Source, msg.Message);
        return Task.CompletedTask;
    }
}
