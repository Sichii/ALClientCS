using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Abstractions
{
    /// <summary>
    ///     Provides a base for records that have <see cref="ALAttribute" />s.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IAttributed" />
    public abstract record AttributedRecordBase : IAttributed
    {
        public float APiercing { get; init; }
        [JsonProperty]
        public float Armor { get; protected set; }

        [JsonProperty]
        public float Attack { get; protected set; }

        [JsonIgnore]
        public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; } = new Dictionary<ALAttribute, float>();

        public float Awesomeness { get; init; }

        public float Blast { get; init; }

        public float Bling { get; init; }

        public float Charisma { get; init; }

        public float Crit { get; init; }

        public float CritDamage { get; init; }

        public float Cuteness { get; init; }

        public float Dex { get; init; }

        public float DReturn { get; init; }

        public float Evasion { get; init; }

        public float Explosion { get; init; }

        [JsonProperty("firesistance")]
        public float FireResistance { get; init; }

        public float For { get; init; }

        [JsonProperty("fzresistance")]
        public float FreezeResistance { get; init; }

        [JsonProperty]
        public float Frequency { get; protected set; }

        [JsonProperty("frequencym")]
        public float FrequencyMod { get; init; }

        public float Gold { get; init; }

        public float GoldSteal { get; init; }

        [JsonProperty("healm")]
        public float HealMod { get; init; }

        [JsonProperty]
        public float HP { get; protected set; }

        public float Int { get; init; }

        public float Lifesteal { get; init; }

        public float Luck { get; init; }

        public float ManaSteal { get; init; }

        public float Miss { get; init; }

        [JsonProperty]
        public float MP { get; protected set; }

        [JsonProperty("mp_cost")]
        public float MPCost { get; init; }

        [JsonProperty("mp_reduction")]
        public float MPReduction { get; init; }

        public float Output { get; init; }

        [JsonProperty("pnresistance")]
        public float PoisonResistance { get; init; }

        [JsonProperty("potionsm")]
        public float PotionsMod { get; init; }

        [JsonProperty]
        public float Range { get; protected set; }

        public float Reflection { get; init; }

        [JsonProperty]
        public float Resistance { get; protected set; }

        public float RPiercing { get; init; }

        [JsonProperty]
        public float Speed { get; protected set; }

        public float Stat { get; init; }

        public float Str { get; init; }

        [JsonProperty("stun")]
        public float StunChance { get; init; }

        public float Vit { get; init; }

        [JsonProperty]
        public float XP { get; protected set; }
    }
}