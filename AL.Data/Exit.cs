using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.Data
{
    /// <summary>
    ///     Represents a point on a map that will teleport you to another map or point.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Exit : ILocation
    {
        /// <summary>
        ///     The accessor (not name or key) of the map this exit leads to.
        /// </summary>
        public string DestinationMap { get; }

        /// <summary>
        ///     The id of the spawn this map leads to.
        /// </summary>
        public int DestinationSpawnId { get; }

        public string Map { get; } = null!;

        /// <summary>
        ///     The type of exit. (door, npc)
        /// </summary>
        public ExitType Type { get; }
        public float X { get; }
        public float Y { get; }

        internal Exit(
            IPoint point,
            string map,
            string destinationMap,
            int destinationSpawnId,
            ExitType type)
        {
            X = point.X;
            Y = point.Y;
            Map = map;
            DestinationMap = destinationMap;
            DestinationSpawnId = destinationSpawnId;
            Type = type;
        }
    }
}