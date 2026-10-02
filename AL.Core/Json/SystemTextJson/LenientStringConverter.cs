#region
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Coerces a JSON number or boolean to a <see cref="string" /> member. The server sends some string fields bare, such
///     as an account <c>owner</c> id or a client-event <c>cevent</c> tag.
/// </summary>
public sealed class LenientStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null   => null,

            //the number's exact wire text
            JsonTokenType.Number => Encoding.UTF8.GetString(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan),

            //capitalized, as Convert.ToString(bool) writes it
            JsonTokenType.True  => "True",
            JsonTokenType.False => "False",
            _                   => throw new JsonException($"Cannot convert {reader.TokenType} to string.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}