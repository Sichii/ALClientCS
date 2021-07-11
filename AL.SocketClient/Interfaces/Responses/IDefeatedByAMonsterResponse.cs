using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface IDefeatedByAMonsterResponse : IGameResponse
    {
        [JsonProperty("monster")]
        string MonsterName { get; }
        [JsonProperty("xp")]
        int XPLost { get; }
    }
}