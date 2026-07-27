#region
using AL.Core.Definitions;
#endregion

namespace AL.Core.Interfaces;

/// <summary>
///     Represents an object that has <see cref="ALAttribute" />s.
/// </summary>
public interface IAttributed
{
    public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; }

    public float APiercing { get; }

    public float Armor { get; }

    public float Attack { get; }

    public float Awesomeness { get; }

    public float Blast { get; }

    public float Bling { get; }

    public float Charisma { get; }

    public float Crit { get; }

    public float CritDamage { get; }

    public float Cuteness { get; }

    public float Dex { get; }

    public float DReturn { get; }

    public float Evasion { get; }

    public float Explosion { get; }

    public float FireResistance { get; }

    public float For { get; }

    public float FreezeResistance { get; }

    public float Frequency { get; }

    public float FrequencyMod { get; }

    public float Gold { get; }

    public float GoldSteal { get; }

    public float HealMod { get; }

    public float HP { get; }

    public float Int { get; }

    public float Lifesteal { get; }

    public float Luck { get; }

    public float ManaSteal { get; }

    public float Miss { get; }

    public float MP { get; }

    public float MPCost { get; }

    public float MPReduction { get; }

    public float Output { get; }

    public float PoisonResistance { get; }

    public float PotionsMod { get; }

    public float Range { get; }

    public float Reflection { get; }

    public float Resistance { get; }

    public float RPiercing { get; }

    public float Speed { get; }

    public float Stat { get; }

    public float Str { get; }

    public float StunChance { get; }

    public float Vit { get; }

    public float XP { get; }
}