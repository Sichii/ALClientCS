#region
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using AL.Data.Monsters;
using Newtonsoft.Json;
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
    ///     The number of this monster that spawns on this map.
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
    ///     Whether or not this monster is required to be killed to access an area.
    /// </summary>
    public bool GateKeeper { get; init; }

    /// <summary>
    ///     Whether or not this monster levels up on it's own, without having to kill players.
    /// </summary>
    public bool Grow { get; init; }

    /// <summary>
    ///     The name of this monster.
    /// </summary>
    [JsonProperty("type")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     TODO: Unknown
    /// </summary>
    public int Radius { get; init; }

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, specifies the boundary in which the entire spawn of this monster will swarm you at
    ///     <see cref="GMonster.ChargeSpeed" />.
    /// </summary>
    [JsonProperty("rage"), JsonConverter(typeof(MapRectangleConverter))]
    public MapRectangle? RageRect { get; init; }

    /// <summary>
    ///     Whether or not this monster roams outside of it's spawn boundary.
    /// </summary>
    public bool Roam { get; init; }

    /// <summary>
    ///     Specifies the way in which this monster spawns/respawns.
    /// </summary>
    public SpawnType SpawnType { get; init; }

    /// <summary>
    ///     Whether or not this is a special monster. Generally means the monster is a boss/event monster, and/or is announced.
    /// </summary>
    public bool Special { get; init; }

    #pragma warning disable 0649
    [JsonProperty("boundaries", ItemConverterType = typeof(MapRectangleConverter))]

    // ReSharper disable once InconsistentNaming
    internal IReadOnlyList<MapRectangle>? _boundaries { get; init; }

    [JsonProperty("boundary"), JsonConverter(typeof(MapRectangleConverter))]

    // ReSharper disable once InconsistentNaming
    internal MapRectangle? _boundary { get; init; }
    #pragma warning restore 0649

    //position
}