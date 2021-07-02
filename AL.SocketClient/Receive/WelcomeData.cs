using AL.APIClient.Definitions;
using AL.Core.Interfaces;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record WelcomeData : ILocation
    {
        public Character Character { get; init; }

        public string GamePlay { get; init; }

        public string In { get; init; }

        public string Map { get; init; }

        [JsonProperty("name")]
        public ServerId Identifier { get; init; }

        public bool PvP { get; init; }

        public ServerRegion Region { get; init; }

        public float X { get; init; }

        public float Y { get; init; }
    }
}