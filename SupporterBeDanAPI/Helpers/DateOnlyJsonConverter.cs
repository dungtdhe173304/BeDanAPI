using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupporterBeDanAPI.Helpers;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateOnly.TryParse(dateString, out var date))
            {
                return date;
            }
            if (DateOnly.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out date))
            {
                return date;
            }
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            return DateOnly.FromDateTime(DateTime.UnixEpoch.AddDays(reader.GetInt32()));
        }

        throw new JsonException($"Unable to parse DateOnly from: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat));
    }
}

public class DateOnlyNullableJsonConverter : JsonConverter<DateOnly?>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }
            if (DateOnly.TryParse(dateString, out var date))
            {
                return date;
            }
            if (DateOnly.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out date))
            {
                return date;
            }
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            return DateOnly.FromDateTime(DateTime.UnixEpoch.AddDays(reader.GetInt32()));
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unable to parse DateOnly? from: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(DateFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
