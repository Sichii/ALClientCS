using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IBuySuccessResponse : INameResponse
    {
        int Cost { get; }
        [JsonProperty("q")]
        int Quantity { get; }
        [JsonProperty("slot")]
        int SlotNum { get; }
        [JsonIgnore]
        string ItemName { get; }
    }
}