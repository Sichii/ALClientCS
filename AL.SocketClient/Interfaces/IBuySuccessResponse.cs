using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
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