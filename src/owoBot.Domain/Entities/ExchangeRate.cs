using System.ComponentModel.DataAnnotations.Schema;

namespace owoBot.Domain.Entities;

[Table(("exchange_rate"))]
public record ExchangeRate
{
    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Column("target")]
    public string Target { get; set; } = string.Empty;

    [Column("rate")]
    public decimal Rate { get; set; }
}
