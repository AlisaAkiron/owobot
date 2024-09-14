using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace owoBot.Domain.Entities;

[Table("cache")]
public record Cache
{
    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("value")]
    public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");
}
