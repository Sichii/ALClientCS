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
    ///     Which drop table a completed fish or dig rolls against (node/server.js:9145).
    /// </summary>
    public DropType Drop { get; init; }

    /// <summary>
    ///     The type of zone.
    /// </summary>
    public ZoneType Type { get; init; }

    /// <summary>
    ///     A polygon representing the bounds of the zone. You work it from beside it, not inside it: the server
    ///     checks a point 24 units away in each of the four directions (node/server.js:9128).
    /// </summary>
    [JsonPropertyName("polygon")]
    public Polygon Vertices { get; init; } = null!;
}