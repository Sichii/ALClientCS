#region
using System;
using System.Collections.Generic;
using System.Linq;
using AL.APIClient.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.APIClient.Json.Converters;

public sealed class LoginResponseConverter : JsonConverter<LoginResponse>
{
    public override LoginResponse? ReadJson(
        JsonReader reader,
        Type objectType,
        LoginResponse? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return default;

        var token = serializer.Deserialize<JToken>(reader);

        if (token == null)
            return default;

        //a successful login is an array of notifications, a failure is an object carrying failed/reason
        //that may or may not wrap that same array - accept either shape rather than indexing positionally
        var envelope = token as JObject;
        var notifications = ((envelope?["infs"] ?? token) as JArray)?.OfType<JObject>()
                                                                    .ToList()
                            ?? [];

        if (envelope != null)
            notifications.Insert(0, envelope);

        return new LoginResponse
        {
            Failed = envelope?.Value<bool>("failed") ?? false,
            Reason = envelope?.Value<string>("reason"),
            Message = FirstValue(notifications, "message"),
            Type = FirstValue(notifications, "type"),
            Html = FirstValue(notifications, "html")
        };
    }

    public override void WriteJson(JsonWriter writer, LoginResponse? value, JsonSerializer serializer)
        => throw new NotImplementedException();

    private static string? FirstValue(IEnumerable<JObject> notifications, string propertyName)
        => notifications.Select(notification => notification.Value<string>(propertyName))
                        .FirstOrDefault(value => !string.IsNullOrEmpty(value));
}
