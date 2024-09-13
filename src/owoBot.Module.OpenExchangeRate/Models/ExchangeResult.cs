using System.Text;

namespace owoBot.Module.OpenExchangeRate.Models;

public record ExchangeResult
{
    public bool IsSuccess { get; init; }

    public string Source { get; init; } = string.Empty;

    public string Target { get; init; } = string.Empty;

    public decimal SourceAmount { get; init; }

    public decimal TargetAmount { get; init; }

    public List<IntermediaExchangeResult>? IntermediateResults { get; init; }

    public string? ErrorMessage { get; init; }

    public bool HasIntermediateResults => IntermediateResults is not null && IntermediateResults.Count != 0;

    public static ExchangeResult Success(
        decimal sourceAmount, string source,
        decimal targetAmount, string target,
        List<IntermediaExchangeResult>? intermediateResults = null)
        => new()
        {
            IsSuccess = true,
            Source = source,
            Target = target,
            SourceAmount = sourceAmount,
            TargetAmount = targetAmount,
            IntermediateResults = intermediateResults
        };

    public static ExchangeResult Fail(decimal sourceAmount, string source, string target, string errorMessage)
        => new()
        {
            IsSuccess = false,
            Source = source,
            Target = target,
            SourceAmount = sourceAmount,
            ErrorMessage = errorMessage
        };

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsSuccess is false)
        {
            return ErrorMessage ?? "Unknown error";
        }

        var sb = new StringBuilder();
        sb.Append($"{SourceAmount:C} {Source}");
        if (HasIntermediateResults)
        {
            foreach (var result in IntermediateResults!)
            {
                sb.Append($" -> {result.Amount:C} {result.Currency}");
            }
        }
        sb.Append($" -> {TargetAmount:C} {Target}");

        return sb.ToString();
    }
}

public record IntermediaExchangeResult(decimal Amount, string Currency);
