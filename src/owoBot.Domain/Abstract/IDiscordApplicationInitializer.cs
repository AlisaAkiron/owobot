using Discord.Rest;
using Discord.WebSocket;

namespace owoBot.Domain.Abstract;

public interface IDiscordApplicationInitializer
{
    public Task SocketInitializer(DiscordSocketClient discordSocketClient);

    public Task RestInitializer(DiscordRestClient discordRestClient);
}
