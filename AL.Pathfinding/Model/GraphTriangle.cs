using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Model
{
    public class GraphTriangle : ILocation, ITriangle, IGenericTriangle<GraphNode2>
    {
        private readonly IReadOnlyList<GraphNode2> _nodeVertices;
        private readonly IReadOnlyList<ILocation> _vertices;

        public string Map { get; }

        public float X { get; }
        public float Y { get; }

        IReadOnlyList<GraphNode2> IGenericTriangle<GraphNode2>.Vertices => _nodeVertices;

        IReadOnlyList<IPoint> IPolygon.Vertices => _vertices;

        public GraphTriangle(ILocation centroid, IReadOnlyList<GraphNode2> nodeVertices)
        {
            Map = centroid.Map;
            X = centroid.X;
            Y = centroid.Y;
            _nodeVertices = nodeVertices;
            _vertices = _nodeVertices.Select(n => n.Vertex).ToArray();
        }

        public bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

        public bool Equals(IGenericTriangle<GraphNode2>? other) => other is not null && _nodeVertices.SequenceEqual(other.Vertices);

        IEnumerator<GraphNode2> IEnumerable<GraphNode2>.GetEnumerator() => _nodeVertices.GetEnumerator();

        IEnumerator<IPoint> IEnumerable<IPoint>.GetEnumerator() => _vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _nodeVertices.GetEnumerator();

        public override int GetHashCode() => HashCode.Combine(_nodeVertices, Map, X, Y);
    }
}