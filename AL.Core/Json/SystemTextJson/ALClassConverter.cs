#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Helpers;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads <see cref="ALClass" />: <c>null</c> or a bool reads as <see cref="ALClass.None" />, and an unknown name or
///     any other token as <see cref="ALClass.NPC" />.
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

        //the server occasionally sends a number or an object here
        reader.Skip();

        return ALClass.NPC;
    }

    public override void Write(Utf8JsonWriter writer, ALClass value, JsonSerializerOptions options) => throw new NotSupportedException();
}