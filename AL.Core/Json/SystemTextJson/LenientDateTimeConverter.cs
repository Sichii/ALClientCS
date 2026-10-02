#region
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a date the server did not write in ISO 8601, which System.Text.Json's built-in reader throws on.
/// </summary>
/// <remarks>
///     The server writes an item's expiry with JavaScript's <c>Date.prototype.toUTCString()</c> (RFC 1123), or an empty
///     string when there is none. Everything is normalised to UTC, since the wire form always is.
/// </remarks>
public sealed class LenientDateTimeConverter : JsonConverter<DateTime?>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, because an absent expiry arrives as an empty string or
    ///     null rather than being omitted.
    /// </summary>
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