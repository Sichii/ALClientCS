using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    public record Location(float X, float Y, string Map) : ILocation
    {
        public Location(string map, IPoint point)
            : this(point.X, point.Y, map) { }

        public Location(ILocation location)
            : this(location.X, location.Y, location.Map) { }
    }
}