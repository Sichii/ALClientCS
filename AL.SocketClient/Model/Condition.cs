#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Interfaces;
using AL.SocketClient.Interfaces;
using Chaos.Time.Abstractions;
#endregion

namespace AL.SocketClient.Model;

/// <summary>Represents a buff or debuff.</summary>
/// <seealso cref="AttributedObjectBase" />
public sealed record Condition : AttributedRecordBase, IPingCompensated, IDeltaUpdatable
{
    /// <summary>
    ///     How long before this condition expires in milliseconds.
    /// </summary>
    [JsonPropertyName("ms")]
    public float DurationMs { get; set; }

    /// <summary>
    ///     Gets the amount of milliseconds that have elapsed since a skill was used.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    ///     If populated,
    ///     <br />
    ///     this could be the name of the monster you need to kill for <see cref="AL.Core.Definitions.Condition.MonsterHunt" />
    ///     <b>OR</b> the ID of a coop boss this player is fighting.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    ///     The intensity of the <see cref="AL.Core.Definitions.Condition.Burned" /> condition.
    /// </summary>
    public float Intensity { get; init; }

    /// <summary>
    ///     If populated, the Unix time in milliseconds at which an encouragement bonus stops paying altogether.
    /// </summary>
    /// <remarks>
    ///     Lone Wolf carries none, because it ends when a second character logs in rather than on a clock.
    /// </remarks>
    [JsonPropertyName("expires")]
    public long? Expires { get; init; }

    /// <summary>
    ///     If populated, this encouragement bonus's share of the gold multiplier.
    /// </summary>
    /// <remarks>
    ///     Reading the character's <see cref="Character.Encouragement" /> is the better way to get a rate: the totals there
    ///     are already multiplied out across every active bonus.
    /// </remarks>
    [JsonPropertyName("gold_multiplier")]
    public float? GoldMultiplier { get; init; }

    /// <summary>
    ///     If populated, this encouragement bonus's share of the luck multiplier.
    /// </summary>
    [JsonPropertyName("luck_multiplier")]
    public float? LuckMultiplier { get; init; }

    public bool IsCompensated { get; private set; }

    /// <summary>
    ///     Whether or not this condition is from a monster ability.
    /// </summary>
    [JsonPropertyName("ability")]
    public bool IsMonsterAbility { get; init; }

    /// <summary>
    ///     Wizard: delevel flag, as long as it's on, after every level 1 monster kill monsters of that kind are deleveled -
    ///     only for level 1's
    ///     <br />
    ///     It seems aimed to avoid just killing level 1's and getting the same quest afterwards
    /// </summary>
    [JsonPropertyName("dl")]
    public bool MonstersDeLevel { get; init; }

    /// <summary>
    ///     If populated, the display effect the <see cref="AL.Core.Definitions.Condition.Filter" /> condition applies.
    ///     <br />
    ///     The browser treats <c>scale</c> as the one name that draws nothing of its own, because the resizing in
    ///     <see cref="Scale" /> is the whole effect; every other name is a colour filter over the sprite.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     If populated, the points the server credits this character with on a coop boss: its accumulated damage and healing,
    ///     not a share. Loot on the boss is paid by these.
    ///     <br />
    ///     See <see cref="Id" /> for the ID of the boss.
    /// </summary>
    [JsonPropertyName("p")]
    public float? Points { get; init; }

    /// <summary>
    ///     The remaining number of monsters you need to kill to complete the
    ///     <see cref="AL.Core.Definitions.Condition.MonsterHunt" />.
    /// </summary>
    [JsonPropertyName("c")]
    public float RemainingMonsters { get; init; }

    /// <summary>
    ///     If populated, the anniversary round this <see cref="AL.Core.Definitions.Condition.AnniversaryVisit" /> was issued
    ///     for.
    /// </summary>
    /// <remarks>
    ///     The server issues one invitation per round and honours it against no other, so one left over from an earlier round
    ///     is a kiss that can never land - and it refuses without saying which of its rules it refused on. Compare this
    ///     against the round the event table reports before spending a walk on it.
    /// </remarks>
    [JsonPropertyName("round")]
    public long? Round { get; init; }

    /// <summary>
    ///     If populated, the multiplier the entity's sprite is drawn at, from the
    ///     <see cref="AL.Core.Definitions.Condition.Filter" /> condition. This is how an event's oversized monsters get their
    ///     size, and it replaces the monster's own <c>size</c> rather than multiplying with it.
    /// </summary>
    public float? Scale { get; init; }

    /// <summary>
    ///     If populated, the id of the server for this <see cref="AL.Core.Definitions.Condition.MonsterHunt" />.
    /// </summary>
    [JsonPropertyName("sn")]
    public string? ServerKey { get; init; }

    /// <summary>
    ///     If populated, which ten-day step of the New Player bonus the account is on, counting from 1.
    /// </summary>
    /// <remarks>
    ///     Each step pays less than the one before, so this changing is a rate cut rather than a cosmetic detail.
    /// </remarks>
    [JsonPropertyName("phase")]
    public int? Phase { get; init; }

    /// <summary>
    ///     If populated, the Unix time in milliseconds at which the New Player bonus drops to its next <see cref="Phase" /> .
    /// </summary>
    [JsonPropertyName("phase_ends")]
    public long? PhaseEnds { get; init; }

    /// <summary>
    ///     If populated, the Id of the merchant who cast this <see cref="AL.Core.Definitions.Condition.MLuck" />.
    /// </summary>
    [JsonPropertyName("f")]
    public string? SourceId { get; init; }

    /// <summary>
    ///     If true, this <see cref="AL.Core.Definitions.Condition.MLuck" /> was cast by a merchant owned by that user.
    ///     <br />
    ///     If false, this <see cref="AL.Core.Definitions.Condition.MLuck" /> can be overwritten by any other merchant.
    /// </summary>
    public bool Strong { get; init; }

    /// <summary>
    ///     If populated, the Unix time in milliseconds this condition really ends at, which outlives <see cref="DurationMs" />
    ///     .
    /// </summary>
    /// <remarks>
    ///     Realm Fatigue is the one that carries it. Activity on another realm pushes this out while the condition is already
    ///     running, and coming home does not clear it, so counting <see cref="DurationMs" /> down from when it first appeared
    ///     finishes early.
    /// </remarks>
    [JsonPropertyName("until")]
    public long? Until { get; init; }

    /// <summary>
    ///     If populated, this encouragement bonus's share of the experience multiplier.
    /// </summary>
    /// <remarks>
    ///     The New Player bonus writes 1 here once any character on the account has reached level 80, while still paying its
    ///     gold and luck shares.
    /// </remarks>
    [JsonPropertyName("xp_multiplier")]
    public float? XpMultiplier { get; init; }

    string IMutable.Id => string.Empty;

    /// <summary>Gets the remaining cooldown in milliseconds.</summary>
    public float RemainingMs => DurationMs - (float)Elapsed.TotalMilliseconds;

    public void CompensateOnce(TimeSpan offset)
    {
        if (IsCompensated)
            throw new InvalidOperationException("Object already compensated.");

        IsCompensated = true;
        Elapsed += offset;
    }

    /// <inheritdoc />
    public void Update(TimeSpan delta) => Elapsed += delta;
}