namespace owoBot.Domain.Types;

public readonly struct Currency
{
    public decimal Amount { get; }

    public string FormatedAmount { get; }

    public CurrencyCode Code { get; }

    private Currency(decimal amount, CurrencyCode code)
    {
        Amount = amount;
        Code = code;

        FormatedAmount = FormatAmount();
    }

    public static Currency From(decimal amount, CurrencyCode code)
    {
        return new Currency(amount, code);
    }

    private string FormatAmount()
    {
        var round = Math.Round(Amount, Code.Digits);
        return round.ToString("N" + Code.Digits);
    }
}
