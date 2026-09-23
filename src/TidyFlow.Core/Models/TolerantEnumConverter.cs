using System.Text.Json;
using System.Text.Json.Serialization;

namespace TidyFlow.Core.Models;

/// <summary>
/// Reads enums from strings case-insensitively and falls back to the default value for anything
/// unrecognized, so a hand-edited or older config file never fails to load. Writes camelCase.
/// </summary>
public sealed class TolerantEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String
            && Enum.TryParse(reader.GetString(), ignoreCase: true, out TEnum value)
            && Enum.IsDefined(value))
        {
            return value;
        }

        if (reader.TokenType == JsonTokenType.Number
            && reader.TryGetInt32(out int number)
            && Enum.IsDefined(typeof(TEnum), number))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            reader.Skip();

        return default;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
        writer.WriteStringValue(JsonNamingPolicy.CamelCase.ConvertName(value.ToString()));
}
