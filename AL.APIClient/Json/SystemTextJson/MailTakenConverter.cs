#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     Provides a converter for a mail's <c>taken</c>, which holds a <c>mailclaim_</c> token string while a claim is in
///     flight. A token reads as taken, since no other claim can land until it settles.
/// </summary>
public sealed class MailTakenConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True   => true,
            JsonTokenType.String => !string.IsNullOrEmpty(reader.GetString()),
            JsonTokenType.Number => reader.GetDouble() != 0,
            _                    => false
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}