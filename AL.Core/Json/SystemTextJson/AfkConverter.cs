#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a bool-or-name field where only the presence of a name matters, such as <c>rip</c>: <c>null</c> reads as
///     false, any string as true, and anything else as a bool.
/// </summary>
public sealed class AfkConverter : JsonConverter<bool>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null maps to false.
    /// </summary>
    public override bool HandleNull => true;

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null   => false,
            JsonTokenType.String => true,

            //a throw here would discard the whole socket frame
            JsonTokenType.Number => reader.GetDouble() != 0,
            _                    => reader.GetBoolean()
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => throw new NotSupportedException();
}