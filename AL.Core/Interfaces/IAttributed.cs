using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.Core.Interfaces
{
    public interface IAttributed
    {
        [JsonIgnore]
        public IDictionary<ALAttribute, float> Attributes { get; init; }

        [JsonProperty]
        public float APiercing { get; }
        [JsonProperty]
        public float Armor { get; }

        [JsonProperty]
        public float Attack { get; }

        [JsonProperty]
        public float Awesomeness { get; }

        [JsonProperty]
        public float Blast { get; }

        [JsonProperty]
        public float Bling { get; }

        [JsonProperty]
        public float Charisma { get; }

        [JsonProperty]
        public float Crit { get; }

        [JsonProperty]
        public float CritDamage { get; }

        [JsonProperty]
        public float Cuteness { get; }

        [JsonProperty]
        public float Dex { get; }

        [JsonProperty]
        public float DReturn { get; }

        [JsonProperty]
        public float Evasion { get; }

        [JsonProperty]
        public float Explosion { get; }

        [JsonProperty("firesistance")]
        public float FireResistance { get; }

        [JsonProperty]
        public float For { get; }

        [JsonProperty("fzresistance")]
        public float FreezeResistance { get; }

        [JsonProperty]
        public float Frequency { get; }

        [JsonProperty("frequencym")]
        public float FrequencyMod { get; }

        [JsonProperty]
        public float Gold { get; }

        [JsonProperty]
        public float GoldSteal { get; }

        [JsonProperty("healm")]
        public float HealMod { get; }

        [JsonProperty]
        public float HP { get; }

        [JsonProperty]
        public float Int { get; }

        [JsonProperty]
        public float Lifesteal { get; }

        [JsonProperty]
        public float Luck { get; }

        [JsonProperty]
        public float ManaSteal { get; }

        [JsonProperty]
        public float Miss { get; }

        [JsonProperty]
        public float MP { get; }

        [JsonProperty("mp_cost")]
        public float MpCost { get; }

        [JsonProperty("mp_reduction")]
        public float MPReduction { get; }

        [JsonProperty]
        public float Output { get; }

        [JsonProperty("pnresistance")]
        public float PoisonResistance { get; }

        [JsonProperty("potionsm")]
        public float PotionsMod { get; }

        [JsonProperty]
        public float Range { get; }

        [JsonProperty]
        public float Reflection { get; }

        [JsonProperty]
        public float Resistance { get; }

        [JsonProperty]
        public float RPiercing { get; }

        [JsonProperty]
        public float Speed { get; }

        [JsonProperty]
        public float Stat { get; }

        [JsonProperty]
        public float Str { get; }

        [JsonProperty("stun")]
        public float StunChance { get; }

        [JsonProperty]
        public float Vit { get; }

        [JsonProperty]
        public float XP { get; }
    }
}