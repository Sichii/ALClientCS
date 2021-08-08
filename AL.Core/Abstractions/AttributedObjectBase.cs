using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.Core.Abstractions
{
    /// <summary>
    ///     Provides a base for classes that have <see cref="ALAttribute" />s.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IAttributed" />
    public abstract class AttributedObjectBase : IAttributed
    {
        public float APiercing { get; protected set; }
        public float Armor { get; protected set; }

        public float Attack { get; protected set; }

        //TODO: Should i keep this?
        public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; } = new Dictionary<ALAttribute, float>();

        public float Awesomeness { get; protected set; }

        public float Blast { get; protected set; }

        public float Bling { get; protected set; }

        public float Charisma { get; protected set; }

        public float Crit { get; protected set; }

        public float CritDamage { get; protected set; }

        public float Cuteness { get; protected set; }

        public float Dex { get; protected set; }

        public float DReturn { get; protected set; }

        public float Evasion { get; protected set; }

        public float Explosion { get; protected set; }

        public float FireResistance { get; protected set; }

        public float For { get; protected set; }

        public float FreezeResistance { get; protected set; }

        public float Frequency { get; protected set; }

        public float FrequencyMod { get; protected set; }

        public float Gold { get; protected set; }

        public float GoldSteal { get; protected set; }

        public float HealMod { get; protected set; }

        public float HP { get; protected set; }

        public float Int { get; protected set; }

        public float Lifesteal { get; protected set; }

        public float Luck { get; protected set; }

        public float ManaSteal { get; protected set; }

        public float Miss { get; protected set; }

        public float MP { get; protected set; }

        public float MpCost { get; protected set; }
        public float MPCost { get; protected set; }

        public float MPReduction { get; protected set; }

        public float Output { get; protected set; }

        public float PoisonResistance { get; protected set; }

        public float PotionsMod { get; protected set; }

        public float Range { get; protected set; }

        public float Reflection { get; protected set; }

        public float Resistance { get; protected set; }

        public float RPiercing { get; protected set; }

        public float Speed { get; protected set; }

        public float Stat { get; protected set; }

        public float Str { get; protected set; }

        public float StunChance { get; protected set; }

        public float Vit { get; protected set; }

        public float XP { get; protected set; }
    }
}