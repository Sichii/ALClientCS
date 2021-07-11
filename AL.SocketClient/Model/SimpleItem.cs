using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record SimpleItem
    {
        public string Name { get; init; }
        [JsonProperty("q")]
        public int Quantity { get; init; }
    }
}