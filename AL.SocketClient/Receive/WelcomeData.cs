using AL.APIClient.Definitions;
using AL.Core.Interfaces;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record WelcomeData : ILocation
    {
        [JsonProperty]
        public Character Character { get; init; }
        [JsonProperty]
        public string GamePlay { get; init; }

        [JsonProperty]
        public string In { get; init; }

        [JsonProperty]
        public string Map { get; init; }

        [JsonProperty]
        public bool PvP { get; init; }

        [JsonProperty]
        public ServerRegion Region { get; init; }

        [JsonProperty("name")]
        public ServerId ServerId { get; init; }

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }
    }
}