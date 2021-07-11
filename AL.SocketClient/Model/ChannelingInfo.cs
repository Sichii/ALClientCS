using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record ChannelingInfo
    {
        [JsonProperty]
        public float MS { get; init; }
    }
}