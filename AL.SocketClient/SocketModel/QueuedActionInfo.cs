using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record QueuedActionInfo
    {
        [JsonProperty]
        public QueuedAction Compound { get; init; }
        [JsonProperty]
        public QueuedAction Exchange { get; init; }

        [JsonProperty]
        public QueuedAction Upgrade { get; init; }
    }
}