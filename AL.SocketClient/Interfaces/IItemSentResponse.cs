using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IItemSentResponse : INameResponse
    {
        [JsonProperty("item")]
        string SentItemName { get; }
        [JsonProperty("q")]
        int Quantity { get; }
        [JsonIgnore]
        string SentTo { get; }
    }
}