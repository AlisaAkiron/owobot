using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using owoBot.Domain.Abstract;
using owoBot.Domain.Attributes;
using owoBot.Domain.Constants;

namespace owoBot.Application.Command;

[AutoConstructor]
public partial class CommandInitializer : IDiscordApplicationInitializer
{
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandInitializer> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public Task SocketInitializer(DiscordSocketClient discordSocketClient)
    {
        discordSocketClient.SlashCommandExecuted += HandleSlashCommandAsync;

        return Task.CompletedTask;
    }

    public Task RestInitializer(DiscordRestClient discordRestClient)
    {
        discordRestClient.LoggedIn += async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var registeredModules = await _interactionService.AddModulesAsync(typeof(CommandInitializer).Assembly, scope.ServiceProvider);
            foreach (var module in registeredModules)
            {
                var hasDevOnly = module.Attributes.FirstOrDefault(x => x is DevOnlyAttribute);
                if (hasDevOnly is not null && _hostEnvironment.IsDevelopment() is false)
                {
                    await _interactionService.RemoveModuleAsync(module);
                }
            }

            foreach (var module in _interactionService.Modules)
            {
                _logger.LogInformation("Interaction module registered: {ModuleName}", module.Name);
            }

            await _interactionService.RegisterCommandsGloballyAsync();
        };

        return Task.CompletedTask;
    }


    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private async Task HandleSlashCommandAsync(SocketSlashCommand interaction)
    {
        using var activity = ActivitySources.CommandActivitySource.StartActivity(interaction.CommandName, ActivityKind.Server);

        activity?.AddTag("interaction_id", interaction.Id.ToString());
        activity?.AddTag("command_name", interaction.CommandName);
        activity?.AddTag("command_id", interaction.CommandId.ToString());
        activity?.AddTag("user_id", interaction.User?.Id.ToString());
        activity?.AddTag("guild_id", interaction.GuildId?.ToString());
        activity?.AddTag("channel_id", interaction.Channel?.Id.ToString());

        var ctx = new SocketInteractionContext<SocketSlashCommand>(_discordSocketClient, interaction);

        activity?.AddEvent(new ActivityEvent("Execute"));

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var result = await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            if (result.IsSuccess is false)
            {
                _logger.LogWarning("Error executing command {CommandName}: {ErrorReason}", interaction.CommandName, result.ErrorReason);

                activity?.AddTag("error", result.ErrorReason);
                activity?.SetStatus(ActivityStatusCode.Error);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error executing command {CommandName}", interaction.CommandName);

            activity?.AddTag("exception", e.ToString());
            activity?.SetStatus(ActivityStatusCode.Error);
        }
    }
}
