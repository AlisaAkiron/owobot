using System.ComponentModel.DataAnnotations.Schema;

namespace owoBot.Domain.Entities;

[Table("currency_info")]
public record CurrencyInfo
{
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
