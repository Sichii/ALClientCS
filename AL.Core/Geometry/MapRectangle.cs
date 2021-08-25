using System;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="Geometry.Rectangle" /> <p />
    ///     Can also potentially contain a map name, be sure to check it.
    /// </summary>
    public record MapRectangle : Rectangle, ILocation
    {
        /// <summary>
        ///     A boundary can potentially contain map information.
        /// </summary>
        public string Map { get; init; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapRectangle" /> class.
        /// </summary>
        /// <param name="vertex1">A vertex of the rectangle.</param>
        /// <param name="vertex2">Another vertex of the rectangle. (must be an opposing vertex to #1)</param>
        /// <param name="map">The map.</param>
        /// <exception cref="System.ArgumentNullException">pt1</exception>
        /// <exception cref="System.ArgumentNullException">pt2</exception>
        public MapRectangle(IPoint vertex1, IPoint vertex2, string? map = null)
            : base(vertex1, vertex2)
        {
            if (vertex1 == null)
                throw new ArgumentNullException(nameof(vertex1));

            if (vertex2 == null)
                throw new ArgumentNullException(nameof(vertex2));

            Map = map ?? string.Empty;
        }

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    }
}