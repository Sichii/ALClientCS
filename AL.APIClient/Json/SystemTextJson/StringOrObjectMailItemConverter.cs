#region
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.APIClient.Model;
using AL.Core.Json.SystemTextJson;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     Binds a mail item the server sends either as a JSON object or as a JSON-stringified object
///     (simplify_item) to a <see cref="MailItem" />. The System.Text.Json replacement for the Newtonsoft
///     <c>StringOrObjectMailItemConverter</c>.
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

        //this converter is registered in the shared REST options, so the inner bind must drop it or it re-enters
        //itself on the same MailItem token and stack-overflows (isolation tests never hit this - they registered
        //no options). The node is a plain object by here (the string form was re-parsed above).
        return node.Deserialize<MailItem>(RecursionSafeOptions.Without(options, typeof(StringOrObjectMailItemConverter)));
    }

    public override void Write(Utf8JsonWriter writer, MailItem? value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}
