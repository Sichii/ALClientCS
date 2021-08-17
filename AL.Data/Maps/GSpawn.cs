using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Attributes;

namespace AL.Data.Maps
{
    /// <inheritdoc cref="AL.Core.Geometry.Orientation" />
    /// <param name="Distance">The distance from the spawn in which you can spawn.</param>
    /// <seealso cref="AL.Core.Geometry.Orientation" />
    public record GSpawn(
        float X,
        float Y,
        Direction Direction,
        [property: JsonArrayIndex(3)] float Distance) : Orientation(X, Y, Direction);
}