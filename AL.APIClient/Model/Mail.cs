using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record Mail
    {
        [JsonProperty("fro")]
        public string From { get; init; }
        public string Id { get; init; }
        public string Item { get; init; }
        public string Message { get; init; }
        public bool Sent { get; init; }
        public bool Taken { get; init; }
        public string To { get; init; }
    }
}