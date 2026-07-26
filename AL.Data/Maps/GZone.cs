#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents a special zone on a map.
/// </summary>
public sealed record GZone
{
    /// <summary>
    ///     The type of drop this zone yields.
    /// </summary>
    public DropType Drop { get; init; }

    /// <summary>
    ///     The type of zone.
    /// </summary>
    public ZoneType Type { get; init; }

    /// <summary>
    ///     A polygon representing the bounds of the zone.
    ///     <br />
    /// </summary>
    [JsonPropertyName("polygon")]
    public Polygon Vertices { get; init; } = null!;
}