#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads the bool-or-name fields where only the presence of the name matters -
///     <c>
///         rip
///     </c>
///     , whose string form is a gravestone cosmetic no headless client has a use for:
///     <c>
///         null
///     </c>
///     -&gt; false, any string -&gt; true, otherwise a bool. Named for
///     <c>
///         afk
///     </c>
///     , which it no longer reads: that field needs the names this one throws away, and has moved to
///     <see cref="AfkStateConverter" />.
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