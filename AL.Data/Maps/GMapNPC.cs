#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AL.Core.Geometry;
using AL.Data.NPCs;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents an NPC's static data for a specific map.
/// </summary>
public sealed record GMapNPC
{
    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this is the area in which this NPC roams.
    /// </summary>
    public MapRectangle? Boundary { get; init; }

    /// <summary>
    ///     This NPC's data from <see cref="GameData.NPCs" />.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public GNPC? Data { get; internal set; }

    /// <summary>
    ///     The id/accessor of this NPC.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     Lazy enumeration of the spots this NPC can be at.
    ///     <br />
    ///     If you're familiar with the original form of this data, it's _position(if present) + _positions(if present).
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyList<Location> Locations { get; init; } = new List<Location>();

    /// <summary>
    ///     Whether or not this NPC walks in a loop between it's possible positions.
    /// </summary>
    public bool Loop { get; init; }

    /// <summary>
    ///     The name of this NPC as displayed on the GUI. Sometimes different than the Id.
    /// </summary>

    //the annotated backing field owns the "name" wire key; without this the accessor claims it too and the
    //type throws on resolution. Newtonsoft keeps binding it as before - it reads its own attributes.
    [JsonIgnore]
    public string Name => _name ?? Id;

    #pragma warning disable 0649
    [JsonPropertyName("name")]
    [JsonInclude]
    private string? _name;

    [JsonPropertyName("position")]
    [JsonInclude]

    // ReSharper disable once InconsistentNaming
    internal Orientation? _position { get; init; }

    [JsonPropertyName("positions")]
    [JsonInclude]

    // ReSharper disable once InconsistentNaming
    internal IReadOnlyList<Orientation>? _positions { get; init; }
    #pragma warning restore 0649
}