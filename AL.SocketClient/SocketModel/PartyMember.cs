using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record PartyMember : ILocation
    {
        [JsonProperty("type")]
        public ALClass Class { get; init; }
        public int Gold { get; init; }
        public string In { get; init; }
        public int Level { get; init; }
        public int Luck { get; init; }
        public string Map { get; init; }
        [JsonProperty("l")]
        public int PartyLimit { get; init; }
        public int Share { get; init; }
        public string Skin { get; init; }
        public float X { get; init; }
        public int XP { get; init; }
        public float Y { get; init; }
    }
}