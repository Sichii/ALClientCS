using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record SimplePlayerData
    {
        public int AFK { get; init; }
        public int Age { get; init; }
        [JsonProperty("type")]
        public ALClass Class { get; init; }
        public int Level { get; init; }
        public string Map { get; init; }
        public string Name { get; init; }
        [JsonProperty("party")]
        public string PartyLeader { get; init; }
    }
}