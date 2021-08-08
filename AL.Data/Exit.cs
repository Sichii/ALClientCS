using AL.Core.Definitions;
using AL.Core.Interfaces;

namespace AL.Data
{
    /// <summary>
    ///     Represents a point on a map that will teleport you to another map or point.
    /// </summary>
    /// <seealso cref="ICircle" />
    /// <seealso cref="ILocation" />
    public record Exit : ICircle, ILocation
    {
        public string Map { get; init; } = null!;
        public float Radius { get; init; }
        /// <summary>
        ///     The location this exit leads to.
        /// </summary>
        public ILocation ToLocation { get; init; } = null!;

        public int ToSpawnIndex { get; init; }
        /// <summary>
        ///     The type of exit. (door, npc)
        /// </summary>
        public ExitType Type { get; init; }
        public float X { get; init; }
        public float Y { get; init; }

        public ILocation Center => this;

        internal Exit(
            string map,
            IPoint point,
            ILocation toLocation,
            int toSpawnIndex,
            ExitType type)
        {
            Map = map;
            X = point.X;
            Y = point.Y;
            ToLocation = toLocation;
            ToSpawnIndex = toSpawnIndex;
            Type = type;

            Radius = type switch
            {
                ExitType.Door        => CONSTANTS.DOOR_RANGE,
                ExitType.Transporter => CONSTANTS.TRANSPORTER_RANGE,
                _                    => float.MaxValue
            };
        }

        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

        public virtual bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);

        public override string ToString() => $"{ILocation.ToString(this)} to {ILocation.ToString(ToLocation)}";
    }
}