#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Abstractions;

/// <summary>
///     Provides a base for classes that have <see cref="ALAttribute" />s.
/// </summary>
/// <seealso cref="AL.Core.Interfaces.IAttributed" />
public abstract class AttributedObjectBase : IAttributed
{
    [JsonInclude]
    public float APiercing { get; protected set; }

    [JsonInclude]
    public float Armor { get; protected set; }

    [JsonInclude]
    public float Attack { get; protected set; }

    //TODO: Should i keep this?
    [JsonIgnore]
    public IReadOnlyDictionary<ALAttribute, float> Attributes { get; init; } = new Dictionary<ALAttribute, float>();

    [JsonInclude]
    public float Awesomeness { get; protected set; }

    [JsonInclude]
    public float Blast { get; protected set; }

    [JsonInclude]
    public float Bling { get; protected set; }

    [JsonInclude]
    public float Charisma { get; protected set; }

    [JsonInclude]
    public float Crit { get; protected set; }

    [JsonInclude]
    public float CritDamage { get; protected set; }

    [JsonInclude]
    public float Cuteness { get; protected set; }

    [JsonInclude]
    public float Dex { get; protected set; }

    [JsonInclude]
    public float DReturn { get; protected set; }

    [JsonInclude]
    public float Evasion { get; protected set; }

    [JsonInclude]
    public float Explosion { get; protected set; }

    [JsonPropertyName("firesistance")]
    [JsonInclude]
    public float FireResistance { get; protected set; }

    [JsonInclude]
    public float For { get; protected set; }

    [JsonPropertyName("fzresistance")]
    [JsonInclude]
    public float FreezeResistance { get; protected set; }

    [JsonInclude]
    public float Frequency { get; protected set; }

    [JsonPropertyName("frequencym")]
    [JsonInclude]
    public float FrequencyMod { get; protected set; }

    [JsonInclude]
    public float Gold { get; protected set; }

    [JsonInclude]
    public float GoldSteal { get; protected set; }

    [JsonPropertyName("healm")]
    [JsonInclude]
    public float HealMod { get; protected set; }

    [JsonInclude]
    public float HP { get; protected set; }

    [JsonInclude]
    public float Int { get; protected set; }

    [JsonInclude]
    public float Lifesteal { get; protected set; }

    [JsonInclude]
    public float Luck { get; protected set; }

    [JsonInclude]
    public float ManaSteal { get; protected set; }

    [JsonInclude]
    public float Miss { get; protected set; }

    [JsonInclude]
    public float MP { get; protected set; }

    [JsonInclude]
    public float MPCost { get; protected set; }

    [JsonPropertyName("mp_reduction")]
    [JsonInclude]
    public float MPReduction { get; protected set; }

    [JsonInclude]
    public float Output { get; protected set; }

    [JsonPropertyName("pnresistance")]
    [JsonInclude]
    public float PoisonResistance { get; protected set; }

    [JsonPropertyName("potionsm")]
    [JsonInclude]
    public float PotionsMod { get; protected set; }

    [JsonInclude]
    public float Range { get; protected set; }

    [JsonInclude]
    public float Reflection { get; protected set; }

    [JsonInclude]
    public float Resistance { get; protected set; }

    [JsonInclude]
    public float RPiercing { get; protected set; }

    [JsonInclude]
    public float Speed { get; protected set; }

    [JsonInclude]
    public float Stat { get; protected set; }

    [JsonInclude]
    public float Str { get; protected set; }

    [JsonPropertyName("stun")]
    [JsonInclude]
    public float StunChance { get; protected set; }

    [JsonInclude]
    public float Vit { get; protected set; }

    [JsonInclude]
    public float XP { get; protected set; }
}