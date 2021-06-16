using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record HitData
    {
        [JsonProperty("anim")]
        public string Animation { get; init; }
        public bool Avoid { get; init; }
        public int Damage { get; init; }
        public float DReturn { get; init; }
        public bool Evade { get; init; }
        public string HID { get; init; }
        public string Id { get; init; }
        public bool Kill { get; init; }
        public float LifeSteal { get; init; }
        public float ManaSteal { get; init; }
        public bool Miss { get; init; }
        public string PID { get; init; }
        public string Projectile { get; init; }
        public float Reflect { get; init; }
        public bool Sneak { get; init; }
        public string Source { get; init; }
        public bool Stun { get; init; }
    }
}