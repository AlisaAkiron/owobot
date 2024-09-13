using System.Diagnostics;
using Discord.Interactions;

namespace owoBot.Application.Command.Commands;

public class DevCommands : InteractionModuleBase
{
    [SlashCommand("echo", "Echoes the input")]
    public async Task EchoAsync(string input)
    {
        var e = new ActivityEvent("EchoAsync", tags: new ActivityTagsCollection([new KeyValuePair<string, object?>("input", input)]));
        Activity.Current?.AddEvent(e);
        await RespondAsync(input);
    }
}
