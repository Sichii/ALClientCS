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
    ///     While any monster from this entry is alive, a door marked "protected" on the same instance refuses
    ///     passage with "transport_cant_protection" (node/server.js:5432).
    /// </summary>
    public bool GateKeeper { get; init; }

    /// <summary>
    ///     Keeps the pack full and its members weak. A kill respawns at once while the population is below two
    ///     thirds, two extras spawn below half, and each level gained adds far less than usual
    ///     (node/server.js:11872, :12114, :1656).
    /// </summary>
    public bool Grow { get; init; }

    /// <summary>
    ///     The name of this monster.
    /// </summary>
    [JsonPropertyName("type")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     For an entry that gives a single position instead of a boundary, the half-width of the square the
    ///     monster spawns and wanders in around it (node/server.js:12000, :12982).
    /// </summary>
    public int Radius { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, standing anywhere inside this rectangle makes every monster from this entry drop what
    ///     it was doing and come for you at <see cref="GMonster.ChargeSpeed" />. Checked once every 4.2 seconds
    ///     per instance, and high enough bling against cuteness can cancel it (node/server.js:12432).
    /// </summary>
    [JsonPropertyName("rage")]
    public MapRectangle? RageRect { get; init; }

    /// <summary>
    ///     Whether or not this monster roams outside of it's spawn boundary.
    /// </summary>
    public bool Roam { get; init; }

    /// <summary>
    ///     Specifies the way in which this monster spawns/respawns. "randomrespawn" picks one of the entry's
    ///     boundaries at random each time, which chooses the map as well as the rectangle (node/server.js:11944).
    /// </summary>
    [JsonPropertyName("stype")]
    public SpawnType SpawnType { get; init; }

    /// <summary>
    ///     If true, monsters from this entry never respawn on their own once killed, the same as
    ///     <see cref="GMonster.Special" /> but decided per map rather than per monster (node/server.js:11866).
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