using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record ActionData : IPoint
    {
        [JsonProperty]
        public string Attacker { get; init; }

        [JsonProperty]
        public float Damage { get; init; }

        [JsonProperty]
        public float ETA { get; init; }

        [JsonProperty]
        public float Heal { get; init; }

        [JsonProperty]
        public float M { get; init; }

        [JsonProperty]
        public string PID { get; init; }

        [JsonProperty]
        public string Projectile { get; init; }

        [JsonProperty]
        public string Source { get; init; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public string Type { get; init; }

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }
    }
}