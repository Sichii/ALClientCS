using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IItemSentResponse : INameResponse
    {
        [JsonProperty("q")]
        int Quantity { get; }
        [JsonProperty("item")]
        string SentItemName { get; }
        [JsonIgnore]
        string SentTo { get; }
    }
}