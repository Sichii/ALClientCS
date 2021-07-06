using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(EventAndBossDataConverter))]
    public record EventAndBossData : EventAndBossInfo;
}