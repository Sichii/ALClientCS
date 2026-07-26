#region
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads
///     <c>
///         Player.AFK
///     </c>
///     , which the server sends inconsistently:
///     <c>
///         null
///     </c>
///     -&gt; false, any string (a timestamp/code) -&gt; true, otherwise a bool. The System.Text.Json replacement for the
///     Newtonsoft
///     <c>
///         AfkConverter
///     </c>
///     .
/// </summary>
public sealed class AfkConverter : JsonConverter<bool>
{
    //a JSON null must reach Read so it maps to false instead of throwing (value type)
    public override bool HandleNull => true;

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null   => false,
            JsonTokenType.String => true,

            //Newtonsoft's Deserialize<bool> coerced a number via Convert.ToBoolean (nonzero -> true); a throw
            //here would discard the whole socket frame, the exact failure this tolerant converter prevents
            JsonTokenType.Number => reader.GetDouble() != 0,
            _                    => reader.GetBoolean()
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => throw new NotSupportedException();
}