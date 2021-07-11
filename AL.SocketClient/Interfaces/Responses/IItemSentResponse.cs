using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
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