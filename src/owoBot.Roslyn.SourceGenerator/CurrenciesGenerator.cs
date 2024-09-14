using System.Globalization;
using System.Text;
using CsvHelper;
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
        using var textStream = new StringReader(text);
        using var csv = new CsvReader(textStream, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<InternalCsvCurrencyCode>().ToList();

        var codes = records
            .Select(x =>
                $"""
                public static readonly CurrencyCode {x.Code} = new("{x.Code}", "{x.Name}", "{x.Symbol}", "{x.MainCountryOrRegion}", "{x.Flag}", {x.Digits});
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
}

internal sealed record InternalCsvCurrencyCode
{
    public string Code { get; set; }  = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Flag { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public string MainCountryOrRegion { get; set; } = string.Empty;

    public int Digits { get; set; }
}
