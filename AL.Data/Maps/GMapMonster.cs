#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Data.Monsters;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents a monster's static data for a specific map.
/// </summary>
public sealed record GMapMonster
{
    /// <summary>
    ///     The areas this monster will spawn for the current map.
    ///     <br />
    ///     If you're familiar with the original form of this data, it's boundary(if present) + boundaries(if present).
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyList<InscribedBoundary> Boundaries { get; init; } = new List<InscribedBoundary>();

    //polygon
    /// <summary>
    ///     The population the server keeps alive for this entry - a kill is replaced rather than added to.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    ///     This monster's data from <see cref="GameData.Monsters" />
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public GMonster? Data { get; internal set; }

    /// <summary>
    ///     Whether a door marked <c>protected</c> on the same instance refuses passage while any monster from this entry is
    ///     alive.
    /// </summary>
    public bool GateKeeper { get; init; }

    /// <summary>
    ///     Whether the pack is kept full and its members weak. A kill respawns at once while the population is below two
    ///     thirds, two extras spawn below half, and each level gained adds far less than usual.
    /// </summary>
    public bool Grow { get; init; }

    /// <summary>The name of this monster.</summary>
    [JsonPropertyName("type")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     For an entry that gives a single position instead of a boundary, the half-width of the square the monster spawns
    ///     and wanders in around it.
    /// </summary>
    public int Radius { get; init; }

    /// <summary>
    ///     <b>NULLABLE</b>. If populated, standing anywhere inside this rectangle makes every monster from this entry come for
    ///     you at <see cref="GMonster.ChargeSpeed" />. Checked once every 4.2 seconds per instance.
    /// </summary>
    [JsonPropertyName("rage")]
    public MapRectangle? RageRect { get; init; }

    /// <summary>
    ///     Whether or not this monster roams outside of it's spawn boundary.
    /// </summary>
    public bool Roam { get; init; }

    /// <summary>
    ///     The way in which this monster spawns/respawns. <c>randomrespawn</c> picks one of the entry's boundaries, and so its
    ///     map, at random each time.
    /// </summary>
    [JsonPropertyName("stype")]
    public SpawnType SpawnType { get; init; }

    /// <summary>
    ///     If true, monsters from this entry never respawn on their own once killed, the same as
    ///     <see cref="GMonster.Special" /> but decided per map rather than per monster.
    /// </summary>
    public bool Special { get; init; }

    #pragma warning disable 0649
    [JsonPropertyName("boundaries")]
    [JsonInclude]

    // ReSharper disable once InconsistentNaming
    internal IReadOnlyList<MapRectangle>? _boundaries { get; init; }

    [JsonPropertyName("boundary")]
    [JsonInclude]

    // ReSharper disable once InconsistentNaming
    internal MapRectangle? _boundary { get; init; }
    #pragma warning restore 0649

    //position
}