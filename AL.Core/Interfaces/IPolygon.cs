using System.Collections.Generic;

namespace AL.Core.Interfaces
{
    /// <summary>
    /// Represents a polygon, concave or convex.
    /// </summary>
    /// <seealso cref="IEnumerable{T}"/>
    public interface IPolygon : IEnumerable<IPoint>
    {
        /// <summary>
        /// The vertices of the polygon. Must be ordered in a way that you could draw the polygon with them.
        /// </summary>
        IReadOnlyList<IPoint> Vertices { get; }
    }
}