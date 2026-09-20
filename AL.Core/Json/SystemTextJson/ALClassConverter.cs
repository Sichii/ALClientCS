#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Helpers;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Parses <see cref="ALClass" /> with fallbacks:
///     <c>
///         null
///     </c>
///     /bool -&gt; <see cref="ALClass.None" />, a string that parses -&gt; that class, a string that does not parse -&gt;
///     <see cref="ALClass.NPC" />. The System.Text.Json replacement for the Newtonsoft
///     <c>
///         ALClassConverter
///     </c>
///     .
/// </summary>
public sealed class ALClassConverter : JsonConverter<ALClass>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null maps to <see cref="ALClass.None" />.
    /// </summary>
    public override bool HandleNull => true;

    public override ALClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.True or JsonTokenType.False)
            return ALClass.None;

        if (reader.TokenType == JsonTokenType.String)
            return EnumHelper.TryParse(reader.GetString(), out ALClass @class) ? @class : ALClass.NPC;

        //server occasionally sends a number/object here; Newtonsoft's Value<string> failed to parse -> NPC
        reader.Skip();

        return ALClass.NPC;
    }

    public override void Write(Utf8JsonWriter writer, ALClass value, JsonSerializerOptions options) => throw new NotSupportedException();
}