using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.Core.Abstractions
{
    /// <summary>
    ///     Provides a base for records that have <see cref="ALAttribute" />s.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IAttributed" />
    public abstract record AttributedRecordBase : IAttributed
    {
        public float APiercing { get; init; }
        public float Armor { get; protected set; }

        public float Attack { get; protected set; }

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

        public float FireResistance { get; init; }

        public float For { get; init; }

        public float FreezeResistance { get; init; }

        public float Frequency { get; protected set; }

        public float FrequencyMod { get; init; }

        public float Gold { get; init; }

        public float GoldSteal { get; init; }

        public float HealMod { get; init; }

        public float HP { get; protected set; }

        public float Int { get; init; }

        public float Lifesteal { get; init; }

        public float Luck { get; init; }

        public float ManaSteal { get; init; }

        public float Miss { get; init; }

        public float MP { get; protected set; }

        public float MpCost { get; init; }

        public float MPReduction { get; init; }

        public float Output { get; init; }

        public float PoisonResistance { get; init; }

        public float PotionsMod { get; init; }

        public float Range { get; protected set; }

        public float Reflection { get; init; }

        public float Resistance { get; protected set; }

        public float RPiercing { get; init; }

        public float Speed { get; protected set; }

        public float Stat { get; init; }

        public float Str { get; init; }

        public float StunChance { get; init; }

        public float Vit { get; init; }

        public float XP { get; protected set; }
    }
}