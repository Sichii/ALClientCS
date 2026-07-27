#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Abstractions;

/// <summary>
///     Provides a base for records that have <see cref="ALAttribute" />s.
/// </summary>
/// <seealso cref="AL.Core.Interfaces.IAttributed" />
public abstract record AttributedRecordBase : IAttributed
{
    public float APiercing { get; init; }

    [JsonInclude]
    public float Armor { get; protected set; }

    [JsonInclude]
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

    [JsonPropertyName("firesistance")]
    public float FireResistance { get; init; }

    public float For { get; init; }

    [JsonPropertyName("fzresistance")]
    public float FreezeResistance { get; init; }

    [JsonInclude]
    public float Frequency { get; protected set; }

    [JsonPropertyName("frequencym")]
    public float FrequencyMod { get; init; }

    public float Gold { get; init; }

    public float GoldSteal { get; init; }

    [JsonPropertyName("healm")]
    public float HealMod { get; init; }

    [JsonInclude]
    public float HP { get; protected set; }

    public float Int { get; init; }

    public float Lifesteal { get; init; }

    public float Luck { get; init; }

    public float ManaSteal { get; init; }

    public float Miss { get; init; }

    [JsonInclude]
    public float MP { get; protected set; }

    [JsonPropertyName("mp_cost")]
    public float MPCost { get; init; }

    [JsonPropertyName("mp_reduction")]
    public float MPReduction { get; init; }

    public float Output { get; init; }

    [JsonPropertyName("pnresistance")]
    public float PoisonResistance { get; init; }

    [JsonPropertyName("potionsm")]
    public float PotionsMod { get; init; }

    [JsonInclude]
    public float Range { get; protected set; }

    public float Reflection { get; init; }

    [JsonInclude]
    public float Resistance { get; protected set; }

    public float RPiercing { get; init; }

    [JsonInclude]
    public float Speed { get; protected set; }

    public float Stat { get; init; }

    public float Str { get; init; }

    [JsonPropertyName("stun")]
    public float StunChance { get; init; }

    public float Vit { get; init; }

    [JsonInclude]
    public float XP { get; protected set; }
}