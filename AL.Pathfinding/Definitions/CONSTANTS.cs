using AL.Core.Geometry;

namespace AL.Pathfinding.Definitions
{
    public static class CONSTANTS
    {
        /// <summary>
        ///     The heuristic value of a town node connection, as calculated by euclidean distance.
        /// </summary>
        public const float TOWN_HEURISTIC = 500f;

        /// <summary>
        ///     The heuristic value of a transport, door, or leave connection.
        /// </summary>
        public const float TRANSPORT_HEURISTIC = 50f;

        /// <summary>
        ///     The default values of a player bounding base.
        /// </summary>
        public static readonly BoundingBase DEFAULT_BOUNDING_BASE = new(8, 7, 2);
    }
}