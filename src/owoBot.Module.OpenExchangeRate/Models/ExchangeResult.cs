namespace owoBot.Module.OpenExchangeRate.Models;

public record ExchangeResult
{
    public bool IsSuccess { get; init; }

    public string Source { get; init; } = string.Empty;

    public string Target { get; init; } = string.Empty;

    public decimal SourceAmount { get; init; }

    public decimal TargetAmount { get; init; }

    public decimal Rate { get; init; }

    public DateTimeOffset? Time { get; init; }

    public string? ErrorMessage { get; init; }

    public static ExchangeResult Success(
        decimal sourceAmount, string source,
        decimal targetAmount, string target,
        decimal rate, DateTimeOffset time)
        => new()
        {
            IsSuccess = true,
            Source = source,
            Target = target,
            SourceAmount = sourceAmount,
            TargetAmount = targetAmount,
            Rate = rate,
            Time = time
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
}
