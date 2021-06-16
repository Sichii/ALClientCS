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
        public IEnumerable<Monster> Monsters { get; init; }

        [JsonProperty]
        public IEnumerable<PlayerData> Players { get; init; }
        [JsonProperty("type")]
        public EntitiesUpdateType UpdateType { get; init; }
    }
}