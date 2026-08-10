#region
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Geometry;
#endregion

namespace AL.Data.NPCs;

/// <summary>
///     <inheritdoc cref="AttributedRecordBase" />
///     <br />
///     Represents the static data for an NPC.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GNPC : AttributedRecordBase
{
    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this NPC has an aura, and these are the attributes it gives to players standing near it.
    /// </summary>
    public IReadOnlyDictionary<ALAttribute, float>? Aura { get; init; }

    /// <summary>
    ///     If populated, the hex color the client draws this NPC's spoken line in. On main an absent one defaults to
    ///     white.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     This NPC's key in the game's NPC table, always the same as the accessor. It is not the live entity's id -
    ///     that is the NPC's display name.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     If this is true, this is bad/old data that should be ignored.
    /// </summary>
    public bool Ignore { get; init; }

    /// <summary>
    ///     Unknown. Barely any NPC carries one, and nothing in the published server or browser client reads it.
    /// </summary>
    public float Interval { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated this NPC sells items - one entry per slot of its shop window, and a null is an empty slot.
    /// </summary>
    public IReadOnlyList<string?>? Items { get; init; }

    /// <summary>
    ///     The level this NPC spawns at. Most carry none and arrive here as zero; the server spawns those at 100.
    /// </summary>
    public float Level { get; init; }

    /// <summary>
    ///     Every place this NPC stands, gathered from the maps. Maps flagged as ignored contribute nothing, so an NPC
    ///     placed only on those has an empty list.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public IReadOnlyList<Location> Locations { get; internal set; } = new List<Location>();

    /// <summary>
    ///     The name of this NPC as seen on the GUI, and the id its live entity is filed under. Null for the handful
    ///     of NPCs that carry no name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     If this is a bank NPC, this is their bank pack number.
    /// </summary>
    public BankPack Pack { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this NPC is a transporter, and this dictionary contains the places (mapName : spawnId) that this
    ///     NPC can take you.
    /// </summary>
    public IReadOnlyDictionary<string, int>? Places { get; init; }

    /// <summary>
    ///     The quest tag for this NPC. A recipe or an exchangeable item carrying the same tag is handled here rather
    ///     than at the usual place.
    /// </summary>
    public Quest Quest { get; init; }

    /// <summary>
    ///     What this NPC is there to do - sell, bank, transport, and so on.
    /// </summary>
    public NPCRole Role { get; init; }

    /// <summary>
    ///     If populated, the texture of the merchant stand drawn under this NPC. Presentation only.
    /// </summary>
    public string? Stand { get; init; }

    /// <summary>
    ///     If this NPC redeems tokens, which token it takes.
    /// </summary>
    public Token Token { get; init; }
}