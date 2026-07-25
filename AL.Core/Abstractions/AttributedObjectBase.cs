using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Abstractions
{
    /// <summary>
    ///     Provides a base for classes that have <see cref="ALAttribute" />s.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IAttributed" />
    public abstract class AttributedObjectBase : IAttributed
    {
        [JsonProperty]
        public float APiercing { get; protected set; }
        [JsonProperty]
        public float Armor { get; protected set; }

        [JsonProperty]
        public float Attack { get; protected set; }

        //TODO: Should i keep this?
        [JsonIgnore]
        public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; } = new Dictionary<ALAttribute, float>();

        [JsonProperty]
        public float Awesomeness { get; protected set; }

        [JsonProperty]
        public float Blast { get; protected set; }

        [JsonProperty]
        public float Bling { get; protected set; }

        [JsonProperty]
        public float Charisma { get; protected set; }

        [JsonProperty]
        public float Crit { get; protected set; }

        [JsonProperty]
        public float CritDamage { get; protected set; }

        [JsonProperty]
        public float Cuteness { get; protected set; }

        [JsonProperty]
        public float Dex { get; protected set; }

        [JsonProperty]
        public float DReturn { get; protected set; }

        [JsonProperty]
        public float Evasion { get; protected set; }

        [JsonProperty]
        public float Explosion { get; protected set; }

        [JsonProperty("firesistance")]
        public float FireResistance { get; protected set; }

        [JsonProperty]
        public float For { get; protected set; }

        [JsonProperty("fzresistance")]
        public float FreezeResistance { get; protected set; }

        [JsonProperty]
        public float Frequency { get; protected set; }

        [JsonProperty("frequencym")]
        public float FrequencyMod { get; protected set; }

        [JsonProperty]
        public float Gold { get; protected set; }

        [JsonProperty]
        public float GoldSteal { get; protected set; }

        [JsonProperty("healm")]
        public float HealMod { get; protected set; }

        [JsonProperty]
        public float HP { get; protected set; }

        [JsonProperty]
        public float Int { get; protected set; }

        [JsonProperty]
        public float Lifesteal { get; protected set; }

        [JsonProperty]
        public float Luck { get; protected set; }

        [JsonProperty]
        public float ManaSteal { get; protected set; }

        [JsonProperty]
        public float Miss { get; protected set; }

        [JsonProperty]
        public float MP { get; protected set; }

        [JsonProperty]
        public float MPCost { get; protected set; }

        [JsonProperty("mp_reduction")]
        public float MPReduction { get; protected set; }

        [JsonProperty]
        public float Output { get; protected set; }

        [JsonProperty("pnresistance")]
        public float PoisonResistance { get; protected set; }

        [JsonProperty("potionsm")]
        public float PotionsMod { get; protected set; }

        [JsonProperty]
        public float Range { get; protected set; }

        [JsonProperty]
        public float Reflection { get; protected set; }

        [JsonProperty]
        public float Resistance { get; protected set; }

        [JsonProperty]
        public float RPiercing { get; protected set; }

        [JsonProperty]
        public float Speed { get; protected set; }

        [JsonProperty]
        public float Stat { get; protected set; }

        [JsonProperty]
        public float Str { get; protected set; }

        [JsonProperty("stun")]
        public float StunChance { get; protected set; }

        [JsonProperty]
        public float Vit { get; protected set; }

        [JsonProperty]
        public float XP { get; protected set; }
    }
}