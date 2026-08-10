#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     <inheritdoc cref="AL.Core.Interfaces.IOriented" />
/// </summary>
/// <param name="X">
///     The x coordinate.
/// </param>
/// <param name="Y">
///     The y coordinate.
/// </param>
/// <param name="Direction">
///     The direction the object is oriented towards.
/// </param>
/// <param name="Distance">
///     The width of the square you are scattered across on arrival, so you land up to half of it away from
///     <paramref name="X" />/<paramref name="Y" /> on each axis. Zero on most spawns (node/server.js:4133).
/// </param>
/// <seealso cref="AL.Core.Geometry.Orientation" />
public record GSpawn(
    float X,
    float Y,
    Direction Direction,
    [property: JsonArrayIndex(3)]
    float Distance) : Orientation(X, Y, Direction);