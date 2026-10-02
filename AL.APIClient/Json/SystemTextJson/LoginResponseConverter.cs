#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.APIClient.Response;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     Provides a converter that normalizes the login endpoint's two shapes into a <see cref="LoginResponse" />: success is
///     a bare array of notification objects; failure is an object carrying <c>failed</c> / <c>reason</c> that may wrap
///     that array under <c>infs</c>.
/// </summary>
public sealed class LoginResponseConverter : JsonConverter<LoginResponse>
{
    private static string? FindFirstValue(IEnumerable<JsonObject> notifications, string propertyName)
        => notifications.Select(notification => notification[propertyName]
                            ?.GetValue<string>())
                        .FirstOrDefault(value => !string.IsNullOrEmpty(value));

    public override LoginResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        var node = JsonNode.Parse(ref reader);

        if (node is null)
            return default;

        var envelope = node as JsonObject;

        var notifications = ((envelope?["infs"] ?? node) as JsonArray)?.OfType<JsonObject>()
                                                                      .ToList()
                            ?? [];

        if (envelope is not null)
            notifications.Insert(0, envelope);

        return new LoginResponse
        {
            Failed = envelope?["failed"]
                         ?.GetValue<bool>()
                     ?? false,
            Reason = envelope?["reason"]
                ?.GetValue<string>(),
            Message = FindFirstValue(notifications, "message"),
            Type = FindFirstValue(notifications, "type"),
            Html = FindFirstValue(notifications, "html")
        };
    }

    public override void Write(Utf8JsonWriter writer, LoginResponse value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}