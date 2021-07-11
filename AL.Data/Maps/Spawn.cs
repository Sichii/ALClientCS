using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Attributes;

namespace AL.Data.Maps
{
    /// <inheritdoc cref="Orientation" />
    /// <param name="Distance">The distance from the spawn in which you can spawn.</param>
    /// <seealso cref="Orientation" />
    public record Spawn(
        float X,
        float Y,
        Direction Direction,
        [property: JsonArrayIndex(3)] float Distance) : Orientation(X, Y, Direction);
}