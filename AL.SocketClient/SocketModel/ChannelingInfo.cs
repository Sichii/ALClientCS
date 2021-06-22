using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record ChannelingInfo
    {
        [JsonProperty]
        public float MS { get; init; }
    }
}