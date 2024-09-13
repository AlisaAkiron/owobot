namespace owoBot.Module.OpenExchangeRate.Models;

public record Currency
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
