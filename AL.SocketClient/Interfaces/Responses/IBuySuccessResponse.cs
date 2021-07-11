using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface IBuySuccessResponse : INameResponse
    {
        int Cost { get; }
        [JsonIgnore]
        string ItemName { get; }
        [JsonProperty("q")]
        int Quantity { get; }
        [JsonProperty("slot")]
        int SlotNum { get; }
    }
}