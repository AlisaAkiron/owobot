using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using owoBot.Application.Command.Services;
using owoBot.Domain.Extensions;

namespace owoBot.Application.Command.Interactions;

[AutoConstructor]
public partial class ExchangeSlashCommand : InteractionModuleBase
{
    private readonly ExchangeRateService _exchangeRateService;

    [SlashCommand("list-currencies", "Get all available currencies")]
    public async Task ListCurrenciesAsync(
        [Summary(description: "page number, 20 elements per page")] int page = 1,
        [Summary(description: "code search pattern, case insensitive, _ matches single char, % matches 0 or more chars")] string? codePattern = null,
        [Summary(description: "name search pattern, case insensitive, _ matches single char, % matches 0 or more chars")] string? namePattern = null)
    {
        var currencies = await _exchangeRateService.GetSupportedCurrenciesAsync(codePattern, namePattern);
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
        [Summary("source-currency", "source currency code"), Autocomplete] string sourceCurrency,
        [Summary("target-currency", "target currency code"), Autocomplete] string targetCurrency)
    {
        var supportedCurrencies = await _exchangeRateService.GetSupportedCurrenciesAsync();
        if (supportedCurrencies.Any(x => x.Code.Equals(sourceCurrency, StringComparison.OrdinalIgnoreCase)) is false)
        {
            await RespondAsync($"Unsupported source currency code: {sourceCurrency}");
            return;
        }
        if (supportedCurrencies.Any(x => x.Code.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase)) is false)
        {
            await RespondAsync($"Unsupported target currency code: {targetCurrency}");
            return;
        }

        var sourceMoney = supportedCurrencies.First(x => x.Code.Equals(sourceCurrency, StringComparison.OrdinalIgnoreCase)).Have(amount);
        var targetCurrencyCode = supportedCurrencies.First(x => x.Code.Equals(targetCurrency, StringComparison.OrdinalIgnoreCase));
        var result = await _exchangeRateService.DirectExchangeAsync(sourceMoney.Currency, targetCurrencyCode);

        if (result is null)
        {
            await RespondAsync("Exchange rate not found");
            return;
        }

        var (exchangeRate, lastUpdateTime) = result.Value;
        var targetMoney = targetCurrencyCode.Have(exchangeRate.Rate * amount);

        var msg = $"{sourceMoney.FormatedAmountWithCurrency} = {targetMoney.FormatedAmountWithCurrency}";

        var currencyNameFields = new StringBuilder();
        currencyNameFields.AppendLine(sourceMoney.Currency.DisplayFormat);
        currencyNameFields.AppendLine(targetMoney.Currency.DisplayFormat);

        var embed = new EmbedBuilder()
            .WithTitle("Exchange Result")
            .WithDescription(msg)
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("Rates")
                    .WithValue($"{sourceMoney.Currency}:{targetMoney.Currency} = {exchangeRate.Rate:0.##}"),
                new EmbedFieldBuilder()
                    .WithName("Currencies")
                    .WithValue(currencyNameFields.ToString()),
                new EmbedFieldBuilder()
                    .WithName("Remarks")
                    .WithValue($"Update Time: {lastUpdateTime:R}"))
            .WithFooter("Powered by ExchangeRate-Api")
            .WithColor(Color.Blue)
            .Build();

        await RespondAsync(embed: embed);
    }

    [AutocompleteCommand("source-currency", "exchange")]
    public async Task AutocompleteExchangeSourceCurrency()
    {
        var userInput = (Context.Interaction as SocketAutocompleteInteraction)?.Data.Current.Value.ToString();

        var results = await GetCurrencyAutocompleteResults(userInput);

        var task = (Context.Interaction as SocketAutocompleteInteraction)?.RespondAsync(results.Take(25));

        if (task is not null)
        {
            await task;
        }
    }

    [AutocompleteCommand("target-currency", "exchange")]
    public async Task AutocompleteExchangeTargetCurrency()
    {
        var userInput = (Context.Interaction as SocketAutocompleteInteraction)?.Data.Current.Value.ToString();

        var results = await GetCurrencyAutocompleteResults(userInput);

        var task = (Context.Interaction as SocketAutocompleteInteraction)?.RespondAsync(results.Take(25));

        if (task is not null)
        {
            await task;
        }
    }

    private async Task<List<AutocompleteResult>> GetCurrencyAutocompleteResults(string? userInput)
    {
        var pattern = string.IsNullOrEmpty(userInput) ? null : $"{userInput}%";
        var supportedCurrencies = await _exchangeRateService.GetSupportedCurrenciesAsync(pattern);
        return supportedCurrencies
            .Select(x => new AutocompleteResult($"{x} - {x.Name}", x.Code))
            .ToList();
    }
}
