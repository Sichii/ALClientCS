#region
using System;
using AL.APIClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.APIClient.Json.Converters;

/// <summary>
///     Binds a mail item that the server sends either as a JSON object or as a JSON-stringified object
///     (simplify_item, adventure_functions.js:1393-1404) to a <see cref="MailItem" />.
/// </summary>
public sealed class StringOrObjectMailItemConverter : JsonConverter<MailItem?>
{
    public override MailItem? ReadJson(
        JsonReader reader,
        Type objectType,
        MailItem? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var token = JToken.Load(reader);

        //the string form is a JSON.stringify'd item; re-parse it into the object it represents
        if (token.Type == JTokenType.String)
        {
            var raw = token.Value<string>();

            if (string.IsNullOrEmpty(raw))
                return null;

            token = JToken.Parse(raw);
        }

        return token.ToObject<MailItem>(serializer);
    }

    public override void WriteJson(JsonWriter writer, MailItem? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}
