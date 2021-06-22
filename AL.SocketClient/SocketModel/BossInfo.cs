using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record BossInfo
    {
        [JsonProperty]
        public float HP { get; init; }

        [JsonProperty]
        public bool Live { get; init; }

        [JsonProperty]
        public string Map { get; init; }
        [JsonProperty("max_hp")]
        public float MaxHP { get; init; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }
    }
}