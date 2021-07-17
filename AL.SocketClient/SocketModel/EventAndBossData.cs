using AL.SocketClient.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <seealso cref="EventAndBossInfo" />
    /// <inheritdoc cref="EventAndBossInfo" />
    [JsonConverter(typeof(EventAndBossDataConverter))]
    public record EventAndBossData : EventAndBossInfo;
}