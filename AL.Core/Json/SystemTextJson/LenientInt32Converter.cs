#region
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Rounds a fractional number to an <see cref="int" /> with banker's rounding (<c>GItem.Grade</c> 3.6 -&gt; 4,
///     2.5 -&gt; 2) and coerces a numeric string.
/// </summary>
/// <remarks>
///     Registered only in the attributed-object converter's inner options; the shared socket options must still throw on a
///     fractional int.
/// </remarks>
public sealed class LenientInt32Converter : JsonConverter<int>
{
    private static int ParseString(string? raw)
        => int.TryParse(
            raw,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : (int)Math.Round(double.Parse(raw!, NumberStyles.Float, CultureInfo.InvariantCulture), MidpointRounding.ToEven);

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Number => reader.TryGetInt32(out var value)
                ? value
                : (int)Math.Round(reader.GetDouble(), MidpointRounding.ToEven),
            JsonTokenType.String => ParseString(reader.GetString()),
            _                    => throw new JsonException($"Cannot convert {reader.TokenType} to int.")
        };

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}