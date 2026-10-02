#region
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Coerces a JSON number, or a numeric or boolean string, to a <see cref="bool" />, non-zero reading as true. The
///     server sends several boolean fields as <c>0</c> / <c>1</c> (party <c>leave</c>, queued-action <c>success</c>).
/// </summary>
/// <remarks>
///     A property-level converter, such as <see cref="AfkConverter" /> on <c>rip</c>, still wins over it.
/// </remarks>
public sealed class LenientBooleanConverter : JsonConverter<bool>
{
    private static bool ParseString(string? raw)
    {
        if (bool.TryParse(raw, out var value))
            return value;

        //a numeric string reads as true when non-zero
        return double.TryParse(
            raw,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var number)
            ? number != 0
            : throw new JsonException($"Cannot convert \"{raw}\" to bool.");
    }

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True   => true,
            JsonTokenType.False  => false,
            JsonTokenType.Number => reader.GetDouble() != 0,
            JsonTokenType.String => ParseString(reader.GetString()),
            _                    => throw new JsonException($"Cannot convert {reader.TokenType} to bool.")
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}