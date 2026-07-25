#region
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Coerces a JSON number or boolean to a <see cref="string" /> member, matching Newtonsoft's reader, whose
///     <c>Convert.ToString</c> leniency yields the number's text or <c>"True"</c>/<c>"False"</c> on both the
///     populate and direct paths. The server sends some string-typed fields as a bare number (an account
///     <c>owner</c> id) or a bare boolean (a client-event <c>cevent</c> tag). Registered in the shared options so
///     it applies on the direct socket path too.
/// </summary>
public sealed class LenientStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null   => null,

            //the number's exact wire text, so an integer id round-trips identically to Newtonsoft's coercion
            JsonTokenType.Number => Encoding.UTF8.GetString(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan),

            //Newtonsoft's Convert.ToString(bool) capitalizes; match it so a coerced value is byte-identical
            JsonTokenType.True  => "True",
            JsonTokenType.False => "False",
            _                   => throw new JsonException($"Cannot convert {reader.TokenType} to string.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}
