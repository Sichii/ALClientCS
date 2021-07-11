using System;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="Geometry.Rectangle" /> <p />
    ///     Can also potentially contain a map name, be sure to check it.
    /// </summary>
    public record Boundary : Rectangle
    {
        /// <summary>
        ///     A boundary can potentially contain map information.
        /// </summary>
        public string? Map { get; init; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Boundary" /> class.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="map">The map.</param>
        /// <exception cref="System.ArgumentNullException">pt1</exception>
        /// <exception cref="System.ArgumentNullException">pt2</exception>
        public Boundary(IPoint pt1, IPoint pt2, string? map = null)
            : base(pt1, pt2)
        {
            if (pt1 == null)
                throw new ArgumentNullException(nameof(pt1));

            if (pt2 == null)
                throw new ArgumentNullException(nameof(pt2));

            Map = map;
        }
    }
}