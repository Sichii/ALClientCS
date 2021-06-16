using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public class CosmeticInfo
    {
        [JsonProperty]
        public string Chin { get; init; }

        [JsonProperty]
        public string Face { get; init; }

        [JsonProperty]
        public string Hair { get; init; }

        [JsonProperty]
        public string Hat { get; init; }

        [JsonProperty]
        public string Head { get; init; }

        [JsonProperty]
        public string Makeup { get; init; }

        [JsonProperty]
        public string Upper { get; init; }
    }
}