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
    ///     If true, this ability applies its <see cref="Condition" /> to every player within <see cref="Radius" /> each time
    ///     it comes off cooldown, with no attack roll.
    /// </summary>
    public bool Aura { get; init; }

    /// <summary>
    ///     The condition this ability applies. Only an <see cref="Aura" /> carries one.
    /// </summary>
    public Condition Condition { get; init; }

    /// <summary>
    ///     The cooldown of this ability in milliseconds. A monster spawns with a random fraction of it already elapsed.
    /// </summary>
    [JsonPropertyName("cooldown")]
    public float CooldownMS { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Cursed" />
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability. The server ignores the flag and hard-codes
    ///     <c>putrid</c>, the one ability carrying it, to curse and poison whoever hits the monster.
    /// </summary>
    public bool Curse { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Poisoned" />.
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability. As with <see cref="Curse" />, the server
    ///     ignores the flag.
    /// </summary>
    public bool Poison { get; init; }

    /// <summary>
    ///     Whether or not this ability does pure damage. Nothing reads it; the skill the ability fires carries its own damage
    ///     type.
    /// </summary>
    [JsonPropertyName("pure")]
    public bool PureDamage { get; set; }

    /// <summary>
    ///     If this is an aura ability, this is the radius of the aura.
    ///     <br />
    ///     If this is not an aura, this is the radius of ability effect. Compared against an edge-to-edge hit box separation.
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    ///     On a burn ability, drops the divider the burn stack builds against from 3 to 1.5, so this monster's
    ///     <see cref="AL.Core.Definitions.Condition.Burned" /> gains intensity about twice as fast, without the usual duration
    ///     extension.
    ///     <br />
    ///     That condition will not show up as the <see cref="Condition" /> for this ability.
    /// </summary>
    public bool Unlimited { get; init; }

    /// <summary>
    ///     The magnitude of this ability, from whichever of <c>amount</c>, <c>damage</c> or <c>heal</c> carried it. Zero when
    ///     none did.
    /// </summary>
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