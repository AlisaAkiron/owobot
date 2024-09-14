namespace owoBot.Domain.Types;

public readonly struct Money
{
    public decimal Amount { get; }

    public string FormatedAmount { get; }

    public string FormatedAmountWithCurrency => $"{FormatedAmount} `{Currency.Code}`";

    public CurrencyCode Currency { get; }

    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;

        FormatedAmount = FormatAmount();
    }

    public static Money From(decimal amount, CurrencyCode code)
    {
        return new Money(amount, code);
    }

    private string FormatAmount()
    {
        if (Currency.CanFormat is false)
        {
            throw new InvalidOperationException("Currency is not formattable.");
        }

        var round = Math.Round(Amount, Currency.Digits);
        return round.ToString("N" + Currency.Digits);
    }
}
