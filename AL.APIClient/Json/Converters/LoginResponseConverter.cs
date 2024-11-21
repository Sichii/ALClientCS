#region
using System;
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

        var arr = serializer.Deserialize<JArray>(reader);

        if (arr == null)
            return default;

        return new LoginResponse
        {
            Message = arr.ElementAtOrDefault(0)
                         ?.Value<string>("message"),
            Type = arr.ElementAtOrDefault(0)
                      ?.Value<string>("type"),
            Html = arr.ElementAtOrDefault(1)
                      ?.Value<string>("html")
        };
    }

    public override void WriteJson(JsonWriter writer, LoginResponse? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}