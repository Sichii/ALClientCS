using AL.APIClient.Json.Converters;
using Newtonsoft.Json;

namespace AL.APIClient.Response
{
    [JsonConverter(typeof(LoginResponseConverter))]
    public record LoginResponse
    {
        [JsonProperty("html")]
        public string Html { get; init; }
        [JsonProperty("message")]
        public string Message { get; init; }

        [JsonProperty("type")]
        public string Type { get; init; }
    }
}