using System.Text;
using Discord;
using Discord.Interactions;
using owoBot.Domain.Extensions;
using owoBot.Module.OpenExchangeRate.Extensions;
using owoBot.Module.OpenExchangeRate.Services;

namespace owoBot.Application.Command.Interactions;

[AutoConstructor]
public partial class ExchangeSlashCommand : InteractionModuleBase
{
    private readonly ExchangeRateManager _exchangeRateManager;
    private readonly TimeProvider _timeProvider;

    [SlashCommand("list-currencies", "Get all available currencies")]
    public async Task ListCurrenciesAsync(
        [Summary(description: "page number, 20 elements per page")] int page = 1,
        [Summary(description: "code search pattern, case insensitive, _ matches single char, % matches 0 or more chars")] string? codePattern = null,
        [Summary(description: "name search pattern, case insensitive, _ matches single char, % matches 0 or more chars")] string? namePattern = null)
    {
        var currencies = await _exchangeRateManager.GetCurrencies(codePattern, namePattern);
        var totalPages = currencies.Count / 20 + 1;

        if (currencies.Count == 0)
        {
            await RespondAsync("No currencies found");
            return;
        }

        if (page < 1 || page > totalPages)
        {
            await RespondAsync("Invalid page number");
            return;
        }

        var str = currencies
            .OrderBy(x => x.Code)
            .Skip((page - 1) * 20)
            .Take(20)
            .Select(x => $"`{x.Code}` - {x.Name}")
            .JoinAsString("\n");


        var embed = new EmbedBuilder()
            .WithTitle($"Available Currencies ({page}/{totalPages})")
            .WithDescription(str)
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("exchange", "Exchange currency")]
    public async Task ExchangeAsync(
        [Summary(description: "amount of source currency")] decimal amount,
        [Summary(description: "source currency code")] string sourceCurrency,
        [Summary(description: "target currency code")] string targetCurrency)
    {
        var currencies = await _exchangeRateManager.GetCurrencies();
        if (currencies.Any(x => x.Code.Equals(sourceCurrency, StringComparison.OrdinalIgnoreCase)) is false)
        {
            await RespondAsync($"Invalid source currency code: {sourceCurrency}");
            return;
        }
        if (currencies.Any(x => x.Code.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase)) is false)
        {
            await RespondAsync($"Invalid target currency code: {targetCurrency}");
            return;
        }

        var result = await _exchangeRateManager.ExchangeAsync(amount, sourceCurrency, targetCurrency);
        var msg = result.Format();

        var exchangeRateFields = result
            .Select(x => $"`{x.Source}` -> `{x.Target}` = {x.Rate:0.##} ({x.Time:T})")
            .JoinAsString("\n");
        var allCurrencies = result
            .Select(x => x.Source)
            .Concat(result.Select(x => x.Target))
            .Distinct()
            .ToList();
        var currencyNameFields = allCurrencies
            .Select(x => currencies.First(c => c.Code == x))
            .Select(x => $"`{x.Code}` - {x.Name}")
            .JoinAsString("\n");

        var remarks = new StringBuilder($"Time: {_timeProvider.GetUtcNow():R}");
        if (result.Count > 1)
        {
            remarks.AppendLine();
            remarks.Append("Note: Exchanged to USD first, may have slight difference compared to direct exchange");
        }

        var embed = new EmbedBuilder()
            .WithTitle("Exchange Result")
            .WithDescription(msg)
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("Rates")
                    .WithValue(exchangeRateFields),
                new EmbedFieldBuilder()
                    .WithName("Currencies")
                    .WithValue(currencyNameFields),
                new EmbedFieldBuilder()
                    .WithName("Remarks")
                    .WithValue(remarks.ToString()))
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: embed);
    }
}
