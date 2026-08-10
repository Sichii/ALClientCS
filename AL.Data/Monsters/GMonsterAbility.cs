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
    ///     If true, this ability applies its <see cref="Condition" /> to every player within <see cref="Radius" />
    ///     each time it comes off cooldown, with no attack roll (node/server.js:12655).
    /// </summary>
    public bool Aura { get; init; }

    /// <summary>
    ///     The condition this ability applies. Only an <see cref="Aura" /> carries one.
    /// </summary>
    public Condition Condition { get; init; }

    /// <summary>
    ///     The cooldown of this ability in milliseconds. A monster spawns with a random fraction of it already
    ///     elapsed, so a pack does not fire in unison (node/server.js:12069).
    /// </summary>
    [JsonPropertyName("cooldown")]
    public float CooldownMS { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Cursed" />
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability. The server does not read the
    ///     flag either - it hard-codes the one ability that carries it, "putrid", which curses and poisons
    ///     whoever hits the monster (node/server.js:3651).
    /// </summary>
    public bool Curse { get; init; }

    /// <summary>
    ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Poisoned" />.
    ///     <br />
    ///     This will not show up as the <see cref="Condition" /> for this ability. See <see cref="Curse" /> - the
    ///     same "putrid" branch applies both, and the flag itself is never read.
    /// </summary>
    public bool Poison { get; init; }

    /// <summary>
    ///     Whether or not this ability does pure damage. Descriptive only: nothing reads it, and the skill the
    ///     ability fires already carries its own damage type.
    /// </summary>
    [JsonPropertyName("pure")]
    public bool PureDamage { get; set; }

    /// <summary>
    ///     If this is an aura ability, this is the radius of the aura.
    ///     <br />
    ///     If this is not an aura, this is the radius of ability effect. Compared against an edge-to-edge
    ///     hit box separation, not a centre-to-centre one (js/old_common_functions.js:618).
    /// </summary>
    public float Radius { get; init; }

    /// <summary>
    ///     On a burn ability, drops the divider the burn stack builds against from 3 to 1.5, so this monster's
    ///     <see cref="AL.Core.Definitions.Condition.Burned" /> gains intensity about twice as fast and forgoes the
    ///     duration extension the ordinary divider gets (node/server.js:3636).
    ///     <br />
    ///     That condition will not show up as the <see cref="Condition" /> for this ability.
    /// </summary>
    public bool Unlimited { get; init; }

    /// <summary>
    ///     The magnitude of this ability, whichever of the three wire spellings carried it: "heal", "damage" or
    ///     "amount". Zero when the ability takes none of them.
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