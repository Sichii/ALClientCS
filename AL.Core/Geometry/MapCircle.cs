using System;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     Represents a circle on a map.
    /// </summary>
    /// <seealso cref="Circle" />
    /// <seealso cref="ILocation" />
    public record MapCircle : Circle, ILocation
    {
        public string Map { get; init; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapCircle" /> class.
        /// </summary>
        /// <param name="map">The map the circle is on.</param>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public MapCircle(string map, IPoint center, float radius)
            : base(center, radius) => Map = map;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapCircle" /> class.
        /// </summary>
        /// <param name="location">The location of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public MapCircle(ILocation location, float radius)
            : base(location, radius) => Map = location.Map;

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Map);
    }
}