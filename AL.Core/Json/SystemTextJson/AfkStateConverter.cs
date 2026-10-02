#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads the server's <c>afk</c> field, which is absent, a bool, or the name of what is driving the character, into
///     <see cref="AfkState" />.
/// </summary>
public sealed class AfkStateConverter : JsonConverter<AfkState>
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null maps to <see cref="AfkState.Unknown" />.
    /// </summary>
    public override bool HandleNull => true;

    public override AfkState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null   => AfkState.Unknown,
            JsonTokenType.False  => AfkState.Active,
            JsonTokenType.True   => AfkState.Idle,
            JsonTokenType.String => ReadName(ref reader),

            //a throw here would discard the whole socket frame
            JsonTokenType.Number => reader.GetDouble() != 0 ? AfkState.Idle : AfkState.Active,
            _                    => AfkState.Unknown
        };

    /// <summary>
    ///     Reads one of the two names the server writes; any other name reads as <see cref="AfkState.Idle" />.
    /// </summary>
    /// <param name="reader">The reader, positioned on a string token.</param>
    /// <returns>The state the name stands for.</returns>
    private static AfkState ReadName(ref Utf8JsonReader reader)
    {
        if (reader.ValueTextEquals("bot"))
            return AfkState.Bot;

        if (reader.ValueTextEquals("code"))
            return AfkState.Code;

        return AfkState.Idle;
    }

    public override void Write(Utf8JsonWriter writer, AfkState value, JsonSerializerOptions options) => throw new NotSupportedException();
}