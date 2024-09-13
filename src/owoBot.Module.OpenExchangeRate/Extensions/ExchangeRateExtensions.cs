using System.Text;
using owoBot.Module.OpenExchangeRate.Models;

namespace owoBot.Module.OpenExchangeRate.Extensions;

public static class ExchangeRateExtensions
{
    public static string Format(this List<ExchangeResult> results)
    {
        if (results.Count == 0)
        {
            return "No results found";
        }

        if (results.Any(x => x.IsSuccess is false))
        {
            return results[0].ErrorMessage ?? "Unknown error";
        }

        var sb = new StringBuilder();
        var r1 = results[0];

        sb.Append($"{r1.SourceAmount:0.##} `{r1.Source}` = {r1.TargetAmount:0.##} `{r1.Target}`");

        foreach (var result in results.Skip(1))
        {
            sb.Append($" = {result.TargetAmount:0.##} `{result.Target}`");
        }

        return sb.ToString();
    }
}
