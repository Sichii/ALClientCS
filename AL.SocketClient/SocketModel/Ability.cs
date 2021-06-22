using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record Ability
    {
        public int Amount { get; init; }
        public int Attr0 { get; init; }
        public bool Aura { get; init; }
        [JsonProperty("condition")]
        public string ConditionName { get; init; }
        [JsonProperty("cooldown")]
        public int CooldownMS { get; init; }
        public int Damage { get; init; }
        public int Heal { get; init; }
        public bool Poison { get; init; }
        [JsonProperty("pure")]
        public bool PureDamage { get; init; }
        public int Radius { get; init; }
        [JsonProperty("stun")]
        public int StunMS { get; init; }
        public bool Unlimited { get; init; }
    }
}