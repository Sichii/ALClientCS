using AL.SocketClient.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    [JsonConverter(typeof(EventAndBossDataConverter))]
    public record EventAndBossData : EventAndBossInfo;
}