using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IDefeatedByAMonsterResponse : IGameResponse
    {
        [JsonProperty("monster")]
        string MonsterName { get; }
        [JsonProperty("xp")]
        int XPLost { get; }
    }
}