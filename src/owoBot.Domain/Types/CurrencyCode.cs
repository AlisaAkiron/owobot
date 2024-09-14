namespace owoBot.Domain.Types;

public readonly partial record struct CurrencyCode
{
    public string Code { get; }

    public string Name { get; } = string.Empty;

    public string Flag { get; } = string.Empty;

    public string Symbol { get; } = string.Empty;

    public string CountryOrRegion { get; } = string.Empty;

    public int Digits { get; init; }

    public bool IsValid { get; init; }

    private CurrencyCode(string code)
    {
        Code = code;
    }

    private CurrencyCode(string code, string name, string symbol, string countryOrRegion, string flag, int digits)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        CountryOrRegion = countryOrRegion;
        Flag = flag;
        Digits = digits;
        IsValid = true;
    }

    public Currency Have(decimal amount)
    {
        return Currency.From(amount, this);
    }

    public static CurrencyCode From(string code)
    {
        return new CurrencyCode(code);
    }

    public static implicit operator string(CurrencyCode currencyCode) => currencyCode.Code;

    public override string ToString()
    {
        return Code;
    }
}
