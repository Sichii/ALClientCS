using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IGoldReceivedResponse : INameResponse
    {
        [JsonProperty("gold")]
        int Amount { get; }
        [JsonIgnore]
        string From { get; }
    }
}