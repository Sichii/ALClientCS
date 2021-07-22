using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;

namespace AL.Data
{
    /// <summary>
    ///     Represents a point on a map that will teleport you to another map or point.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Exit : ILocation
    {
        public string Map { get; } = null!;
        /// <summary>
        ///     The location this exit leads to.
        /// </summary>
        public Location ToLocation { get; } = null!;

        /// <summary>
        ///     The type of exit. (door, npc)
        /// </summary>
        public ExitType Type { get; }
        public float X { get; }
        public float Y { get; }

        internal Exit(string map, IPoint point, Location toLocation, ExitType type)
        {
            Map = map;
            X = point.X;
            Y = point.Y;
            ToLocation = toLocation;
            Type = type;
        }
    }
}