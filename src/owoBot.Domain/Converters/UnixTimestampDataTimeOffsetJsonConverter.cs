using System.Text.Json;
using System.Text.Json.Serialization;

namespace owoBot.Domain.Converters;

public class UnixTimestampDataTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        var unixTimestamp = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
