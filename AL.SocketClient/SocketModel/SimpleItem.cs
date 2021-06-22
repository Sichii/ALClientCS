using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record SimpleItem
    {
        public string Name { get; init; }
        [JsonProperty("q")]
        public int Quantity { get; init; }
    }
}