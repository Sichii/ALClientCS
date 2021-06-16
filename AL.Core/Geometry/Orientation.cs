using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;

namespace AL.Core.Geometry
{
    public record Orientation(
        [property: JsonArrayIndex(0)] float X,
        [property: JsonArrayIndex(1)] float Y,
        [property: JsonArrayIndex(2)] Direction Direction) : IOriented
    {
        public static readonly Orientation None = new(float.MaxValue, float.MaxValue, Direction.Invalid);
    }
}