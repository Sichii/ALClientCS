using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record QueuedActionData
    {
        //TODO: What's this?
        public int Num { get; init; }
        [JsonProperty("p")]
        public Prediction Prediction { get; init; }
        [JsonProperty("q")]
        public QueuedActionInfo QueuedActionInfo { get; init; }
    }
}