namespace owoBot.Domain.Types;

public readonly partial record struct CurrencyCode
{
    public string Code { get; }

    public string Name { get; private init; } = string.Empty;

    public int Digits { get; }

    public string DisplayFormat => CanFormat ? $"`{Code}` - {Name}" : throw new NotSupportedException();

    public bool CanFormat { get; }

    private CurrencyCode(string code, string? name = null)
    {
        Code = code;
        Name = name ?? string.Empty;
    }

    private CurrencyCode(string code, string name, int digits)
    {
        Code = code;
        Name = name;
        Digits = digits;
        CanFormat = true;
    }

    public Money Have(decimal amount)
    {
        return Money.From(amount, this);
    }

    public static CurrencyCode From(string code, string? name = null)
    {
        if (AllCurrencies.Any(x => x.Code == code) is false)
        {
            return new CurrencyCode(code, name);
        }

        var currencyCode = AllCurrencies.First(x => x.Code == code);
        if (name is null)
        {
            return currencyCode;
        }

        return currencyCode with { Name = name };
    }

    public static implicit operator string(CurrencyCode currencyCode) => currencyCode.Code;

    public override string ToString()
    {
        return Code.ToUpperInvariant();
    }
}
