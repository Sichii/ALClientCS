using AL.APIClient.Definitions;
using AL.Core.Abstractions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(AttributedObjectConverter<Condition>))]
    public class Condition : AttributedObjectBase
    {
        [JsonProperty]
        public bool Ability { get; init; }

        [JsonProperty]
        public bool DL { get; init; }

        [JsonProperty]
        public string Id { get; init; }

        [JsonProperty]
        public float Intensity { get; init; }

        [JsonProperty]
        public float P { get; init; }

        [JsonProperty("c")]
        public float RemainingMonsters { get; init; }

        [JsonProperty("ms")]
        public float RemainingMS { get; init; }

        [JsonProperty("sn")]
        public ServerId ServerID { get; init; }

        [JsonProperty("f")]
        public string SourceId { get; init; }

        [JsonProperty]
        public bool Strong { get; init; }
    }
}