using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Attributes;

namespace AL.Data.Maps
{
    /// <summary>
    ///     <inheritdoc cref="AL.Core.Interfaces.IOriented" />
    /// </summary>
    /// <param name="X">The x coordinate.</param>
    /// <param name="Y">The y coordinate.</param>
    /// <param name="Direction">The direction the object is oriented towards.</param>
    /// <param name="Distance">The distance from the spawn in which you can spawn.</param>
    /// <seealso cref="AL.Core.Geometry.Orientation" />
    public record GSpawn(
        float X,
        float Y,
        Direction Direction,
        [property: JsonArrayIndex(3)] float Distance) : Orientation(X, Y, Direction);
}