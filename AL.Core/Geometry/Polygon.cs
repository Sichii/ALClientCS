using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Geometry
{
    /// <inheritdoc cref="IPolygon"/>
    /// <seealso cref="IPolygon"/>
    [JsonArray]
    public record Polygon : IPolygon
    {
        public IReadOnlyList<IPoint> Vertices { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon" /> class.
        /// </summary>
        /// <param name="vertices">An ordered list of vertices. (must be ordered to draw the outline of the polygon)</param>
        public Polygon(IEnumerable<IPoint> vertices) => Vertices = vertices.ToList();

        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}