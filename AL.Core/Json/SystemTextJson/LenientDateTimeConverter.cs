#region
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a date the server did not write in ISO 8601. System.Text.Json's built-in reader accepts only ISO 8601 / RFC
///     3339 and throws on anything else, which in a socket frame takes the whole frame down.
/// </summary>
/// <remarks>
///     The server builds an item's expiry with JavaScript's
///     <c>
///         Date.prototype.toUTCString()
///     </c>
///     , which emits RFC 1123 (
///     <c>
///         "Wed, 14 Jun 2017 07:00:00 GMT"
///     </c>
///     ), and substitutes an empty string when the item has no expiry. Newtonsoft's
///     <c>
///         IsoDateTimeConverter
///     </c>
///     absorbed both because it fell through to <see cref="DateTime.Parse(string, IFormatProvider, DateTimeStyles)" />;
///     this reproduces that leniency. Everything is normalised to UTC — the wire form is always UTC, whether or not it
///     says so.
/// </remarks>
public sealed class LenientDateTimeConverter : JsonConverter<DateTime?>
{
    //an absent expiry arrives as "" or null rather than being omitted, so null must reach Read
    public override bool HandleNull => true;

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        //a number is epoch milliseconds; the server sends this shape before it stringifies an expiry
        if (reader.TokenType == JsonTokenType.Number)
            return DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
                                 .UtcDateTime;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Cannot read a date from {reader.TokenType}.");

        //the built-in reader is tried first so a conforming value keeps its exact fast path
        if (reader.TryGetDateTime(out var iso))
            return iso.ToUniversalTime();

        var text = reader.GetString();

        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var parsed))
            return parsed;

        throw new JsonException($@"Cannot read a date from ""{text}"".");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value.ToUniversalTime());
    }
}