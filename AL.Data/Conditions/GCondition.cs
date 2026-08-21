#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
#endregion

namespace AL.Data.Conditions;

/// <summary>
///     Represents a buff or detriment.
///     <br />
///     <inheritdoc cref="AttributedRecordBase" />
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GCondition : AttributedRecordBase
{
    #pragma warning disable 0649
    [JsonPropertyName("can_move")]
    [JsonInclude]
    private bool _canMove;
    #pragma warning restore 0649

    /// <summary>
    ///     Whether or not this condition is associated with an aura.
    /// </summary>
    public bool Aura { get; init; }

    /// <summary>
    ///     Marks a condition that stops you acting entirely - stunned, deep freeze, fingered and stoned are the
    ///     four that carry it.
    /// </summary>
    public bool Blocked { get; init; }

    /// <summary>
    ///     Whether or not this condition is a buff.
    /// </summary>
    public bool Buff { get; init; }

    /// <summary>
    ///     For the condition "Reflection", the ceiling on reflection while it holds. The published server source
    ///     reads it nowhere, so take it as the data's intent rather than an observed rule.
    /// </summary>
    [JsonPropertyName("cap_reflection")]
    public int CapReflection { get; init; }

    /// <summary>
    ///     Marks a channelled action - the town warp, fishing, mining and pickpocket carry it. These sit in the
    ///     character's channelling collection rather than among its conditions.
    /// </summary>
    public bool Channel { get; init; }

    /// <summary>
    ///     Whether or not this condition is a debuff (detrimental).
    /// </summary>
    [JsonPropertyName("debuff")]
    public bool Debuff { get; init; }

    /// <summary>
    ///     If populated, the resistance stat that shrugs this condition off outright. The server rolls it as a
    ///     flat percentage and, on a hit, does not apply the condition at all (node/server_functions.js:2715).
    /// </summary>
    [JsonPropertyName("defense")]
    public ALAttribute Defense { get; init; }

    /// <summary>
    ///     If populated, the floor of a randomised duration in milliseconds, with <see cref="DurationMs" /> as the
    ///     ceiling. For fishing and mining the server reads the matching pair off the skill, not off the condition.
    /// </summary>
    [JsonPropertyName("duration_min")]
    public int DurationMinMs { get; init; }

    /// <summary>
    ///     The duration of this condition in milliseconds. Used whenever whatever applies the condition does not
    ///     name a duration of its own (node/server_functions.js:2714).
    /// </summary>
    [JsonPropertyName("duration")]
    public int DurationMs { get; init; }

    /// <summary>
    ///     The amount this condition heals, once per <see cref="IntervalMS" />. An immune monster heals nothing
    ///     (node/server.js:12522).
    /// </summary>
    public int Heal { get; init; }

    /// <summary>
    ///     Names another condition. Only the fire-essence burn carries it, pointing at a name that is defined
    ///     nowhere in the game data and read nowhere in the server, so nothing acts on it.
    /// </summary>
    public string? Intensity { get; init; }

    /// <summary>
    ///     The time between ticks of this condition in milliseconds.
    /// </summary>
    [JsonPropertyName("interval")]
    public int IntervalMS { get; init; }

    /// <summary>
    ///     The name of this condition. (can be different than the key)
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If true, a cleanse does not strip this condition: both the purify skill and the debuff-clearing
    ///     consumables skip it (node/server.js:3593, :7238).
    /// </summary>
    public bool Persistent { get; init; }

    /// <summary>
    ///     Caps <see cref="ALAttribute.Speed" /> at this value - tangle sets 24, hard shell 10, dash 500. The
    ///     published server source applies it nowhere, so take it as the data's intent, not an observed rule.
    /// </summary>
    [JsonPropertyName("set_speed")]
    public int SetSpeed { get; init; }

    /// <summary>
    ///     If populated, the name of this condition's icon art in <c>G.positions</c>. The browser client draws a
    ///     condition's icon through the item pipeline by this name (js/html.js:439), preferring a skin carried on
    ///     the live condition itself where the wire sends one.
    /// </summary>
    public string? Skin { get; init; }

    /// <summary>
    ///     Marks a bookkeeping condition such as an unverified account. The client lists these on their own,
    ///     apart from buffs and debuffs (js/html.js:4869).
    /// </summary>
    public bool Technical { get; init; }

    /// <summary>
    ///     Whether or not there is a GUI element related to this condition.
    /// </summary>
    public bool UI { get; init; }

    /// <summary>
    ///     Whether moving leaves this channelled action running. Moving cancels every channel without it, and
    ///     only the town warp has it (node/server.js:10051). True for anything that is not a channel at all.
    /// </summary>
    [JsonIgnore]
    public bool CanMove => !Channel || _canMove;
}