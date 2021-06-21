using System.Collections.Generic;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record EntitiesData
    {
        [JsonProperty]
        public string In { get; init; }

        [JsonProperty]
        public string Map { get; init; }

        [JsonProperty]
        public IEnumerable<Monster> Monsters { get; init; } = new List<Monster>();

        [JsonProperty]
        public IEnumerable<Player> Players { get; init; } = new List<Player>();
        [JsonProperty("type")]
        public EntitiesUpdateType UpdateType { get; init; }
    }
}