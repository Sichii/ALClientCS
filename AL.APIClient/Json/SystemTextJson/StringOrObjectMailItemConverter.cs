#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.APIClient.Model;
using AL.Core.Json.SystemTextJson;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     Provides a converter that binds a mail item the server sends either as a JSON object or as a JSON-stringified
///     object (<c>simplify_item</c>) to a <see cref="MailItem" />.
/// </summary>
public sealed class StringOrObjectMailItemConverter : JsonConverter<MailItem?>
{
    public override MailItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var node = JsonNode.Parse(ref reader);

        if (node is null)
            return null;

        //the string form is a JSON.stringify'd item; re-parse it into the object it represents
        if (node.GetValueKind() == JsonValueKind.String)
        {
            var raw = node.GetValue<string>();

            if (string.IsNullOrEmpty(raw))
                return null;

            node = JsonNode.Parse(raw);
        }

        //the shared options hold this converter, so the inner bind drops it or it re-enters itself and overflows the stack
        return node.Deserialize<MailItem>(RecursionSafeOptions.Without(options, typeof(StringOrObjectMailItemConverter)));
    }

    public override void Write(Utf8JsonWriter writer, MailItem? value, JsonSerializerOptions options) => throw new NotSupportedException();
}