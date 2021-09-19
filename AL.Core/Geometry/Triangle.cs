using System.Collections;
using System.Collections.Generic;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    public class Triangle : ITriangle
    {
        public IReadOnlyList<IPoint> Vertices { get; }

        public float X { get; }
        public float Y { get; }

        public Triangle(IPoint centroid, IReadOnlyList<IPoint> vertices)
        {
            X = centroid.X;
            Y = centroid.Y;

            Vertices = vertices;
        }

        public bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}