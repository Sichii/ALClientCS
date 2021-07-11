using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record QueuedAction
    {
        [JsonProperty("ms")]
        public float CurrentMS { get; init; }

        [JsonProperty("len")]
        public float LengthMS { get; init; }

        [JsonProperty]
        public float Num { get; init; }
    }
}