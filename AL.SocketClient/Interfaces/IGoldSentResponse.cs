using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IGoldSentResponse : INameResponse
    {
        [JsonProperty("gold")]
        int Amount { get; }
        [JsonIgnore]
        string SentTo { get; }
    }
}