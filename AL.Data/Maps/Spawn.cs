using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Attributes;

namespace AL.Data.Maps
{
    public record Spawn(float X, float Y, Direction Direction, [property: JsonArrayIndex(3)] float Distance) : Orientation(X, Y, Direction);
}