#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     <inheritdoc cref="AttributedRecordBase" />
///     <br />
///     Represents an ability used by a monster.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GMonsterAbility : AttributedRecordBase
{
    /// <summary>
    ///     Whether or not this is an aura ability.
    /// </summary>
    public bool Aura { get; init; }

    /// <summary>
    ///     The condition this ability applies.
    /// </summary>
    public Condition Condition { get; init; }

    /// <summary>
    ///     The cooldown of this ability in milliseconds.
    /// </summary>
    [JsonPropertyName("cooldown")]
    public float CooldownMS { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Cursed" />
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability.
    /// </summary>
    public bool Curse { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Poisoned" />.
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability.
    /// </summary>
    public bool Poison { get; init; }

    /// <summary>
    ///     Whether or not this ability does pure damage.
    /// </summary>
    [JsonPropertyName("pure")]
    public bool PureDamage { get; set; }

    /// <summary>
    ///     If this is an aura ability, this is the radius of the aura.
    ///     <br />
    ///     If this is not an aura, this is the radius of ability effect.
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    ///     Whether or not this ability's condition stacks infinitely.
    ///     <br />
    ///     Currently only used for <see cref="AL.Core.Definitions.Condition.Burned" />, which will not show up as the
    ///     <see cref="Condition" /> for this ability.
    /// </summary>
    public bool Unlimited { get; init; }

    /// <summary>
    ///     The amount of heal/damage/amount for this ability.
    /// </summary>

    //the annotated backing field owns the "amount" wire key; without this the accessor claims it too and the
    //type throws on resolution. Newtonsoft keeps binding it as before - it reads its own attributes.
    [JsonIgnore]
    public float Amount => _amount ?? _damage ?? _heal ?? 0f;
    #pragma warning disable 0649
    [JsonPropertyName("amount")]
    [JsonInclude]
    private float? _amount;

    [JsonPropertyName("damage")]
    [JsonInclude]
    private float? _damage;

    [JsonPropertyName("heal")]
    [JsonInclude]
    private float? _heal;
    #pragma warning restore 0649
}