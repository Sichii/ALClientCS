#region
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Coerces a JSON number (and numeric/boolean string) to a <see cref="bool" />, matching Newtonsoft's reader,
///     which yields <c>Convert.ToBoolean</c> semantics (non-zero → true) on both its populate and direct paths.
///     The server sends several boolean fields as <c>0</c>/<c>1</c> (e.g. party <c>leave</c>, queued-action
///     <c>success</c>). Registered in the shared options so it applies on the direct socket path too; a
///     property-level converter (e.g. AfkConverter for the bool-or-gravestone-name RIP field) still wins over it.
/// </summary>
public sealed class LenientBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True   => true,
            JsonTokenType.False  => false,
            JsonTokenType.Number => reader.GetDouble() != 0,
            JsonTokenType.String => ParseString(reader.GetString()),
            _                    => throw new JsonException($"Cannot convert {reader.TokenType} to bool.")
        };

    private static bool ParseString(string? raw)
    {
        if (bool.TryParse(raw, out var value))
            return value;

        //Newtonsoft also accepts a numeric string here (Convert.ToBoolean over the parsed number)
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number != 0
            : throw new JsonException($"Cannot convert \"{raw}\" to bool.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}
