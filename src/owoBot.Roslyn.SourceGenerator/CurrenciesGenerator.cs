using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace owoBot.Roslyn.SourceGenerator;

[Generator]
public class CurrenciesGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var csvFile = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("currencies.csv", StringComparison.InvariantCultureIgnoreCase))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString());

        context.RegisterSourceOutput(csvFile, (sourceProductionContext, csvContent) =>
        {
            if (string.IsNullOrEmpty(csvContent) is false)
            {
                var generatedCode = GenerateCurrencyCode(csvContent!);
                sourceProductionContext.AddSource("CurrencyCode.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
            }
        });
    }

    private static string GenerateCurrencyCode(string text)
    {
        var records = ParseCsv(text);

        var codes = records
            .Select(x =>
                $"""
                public static readonly CurrencyCode {x.Code} = new("{x.Code}", "{x.Name}", "{x.Symbol}", "{x.CountryOrRegion}", "{x.Flag}", {x.Digits});
                """);

        var sb = new StringBuilder();
        foreach (var code in codes)
        {
            sb.Append("    ");
            sb.AppendLine(code);
        }
        var definitions = sb.ToString();

        return $$"""
               namespace owoBot.Domain.Types;

               public readonly partial record struct CurrencyCode
               {
               {{definitions}}
               }
               """;
    }

    private static List<InternalCsvCurrencyCode> ParseCsv(string text)
    {
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var headers = lines[0].Split(',');

        var currencyCodes = new List<InternalCsvCurrencyCode>();

        for (var i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');

            var code = fields[Array.IndexOf(headers, "Code")];
            var name = fields[Array.IndexOf(headers, "Name")];
            var symbol = fields[Array.IndexOf(headers, "Symbol")];
            var country = fields[Array.IndexOf(headers, "CountryOrRegion")];
            var flag = fields[Array.IndexOf(headers, "Flag")];
            var digits = int.Parse(fields[Array.IndexOf(headers, "Digits")]);

            currencyCodes.Add(new InternalCsvCurrencyCode
            {
                Code = code,
                Name = name,
                Flag = flag,
                Symbol = symbol,
                CountryOrRegion = country,
                Digits = digits
            });
        }

        return currencyCodes;
    }
}

internal sealed record InternalCsvCurrencyCode
{
    public string Code { get; set; }  = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Flag { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public string CountryOrRegion { get; set; } = string.Empty;

    public int Digits { get; set; }
}
