using Discord.Interactions;

namespace owoBot.Application.Command.Interactions;

public class CommonSlashCommands : InteractionModuleBase
{
    [SlashCommand("ping", "Test the responsiveness of the bot")]
    public async Task PingAsync()
    {
        await RespondAsync("pong!", ephemeral: true);
    }
}
