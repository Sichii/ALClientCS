using System;
using System.Collections;
using System.Collections.Generic;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    /// </summary>
    public record InscribedBoundary : ICircle, IRectangle, ILocation
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }

        public string Map { get; }

        public float Radius { get; }
        public float Right { get; }
        public float Top { get; }

        public IReadOnlyList<IPoint> Vertices { get; }
        public float Width { get; }

        public float X { get; }
        public float Y { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InscribedBoundary" /> class.
        /// </summary>
        /// <param name="vertex1">A vertex of the rectangle.</param>
        /// <param name="vertex2">Another vertex of the rectangle. (must be an opposing vertex to #1)</param>
        /// <param name="map">The map.</param>
        /// <exception cref="System.ArgumentNullException">vertex1</exception>
        /// <exception cref="System.ArgumentNullException">vertex2</exception>
        public InscribedBoundary(IPoint vertex1, IPoint vertex2, string map)
        {
            if (vertex1 == null)
                throw new ArgumentNullException(nameof(vertex1));

            if (vertex2 == null)
                throw new ArgumentNullException(nameof(vertex2));

            Map = map;
            X = (vertex1.X + vertex2.X) / 2;
            Y = (vertex1.Y + vertex2.Y) / 2;
            Width = Math.Abs(vertex1.X - vertex2.X);
            Height = Math.Abs(vertex1.Y - vertex2.Y);
            Radius = Math.Min(Width, Height);
            Left = X - Width / 2;
            Top = Y - Height / 2;
            Right = X + Width / 2;
            Bottom = Y + Height / 2;

            Vertices = new List<IPoint>
            {
                new Point(Left, Top), new Point(Right, Top), new Point(Right, Bottom), new Point(Left, Bottom)
            };
        }

        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
        public virtual bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);
        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}