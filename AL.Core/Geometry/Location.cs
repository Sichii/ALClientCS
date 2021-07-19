using System;
using AL.Core.Interfaces;

// ReSharper disable ConstantConditionalAccessQualifier

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="ILocation" />
    /// </summary>
    /// <param name="X">The x coordinate.</param>
    /// <param name="Y">The y coordinate.</param>
    /// <param name="Map">The map name.</param>
    /// <seealso cref="AL.Core.Interfaces.ILocation" />
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Location(float X, float Y, string Map) : ILocation
    {
        /// <summary>
        ///     This represents an invalid value since the default value of a point <c>(0, 0)</c> is a used value.
        /// </summary>
        public static Location None => new(string.Empty, Point.None);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Location" /> class.
        /// </summary>
        /// <param name="map">The map name.</param>
        /// <param name="point">The point.</param>
        /// <exception cref="ArgumentNullException">point</exception>
        public Location(string map, IPoint point)
            : this(point?.X ?? throw new ArgumentNullException(nameof(point)),
                point?.Y ?? throw new ArgumentNullException(nameof(point)), map) { }
    }
}