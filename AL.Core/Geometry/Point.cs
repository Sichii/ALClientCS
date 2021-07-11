using System;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IPoint" />
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public readonly struct Point : IPoint, IEquatable<Point>
    {
        /// <summary>
        ///     This represents an invalid value since the default value of a point <c>(0, 0)</c> is a used value.
        /// </summary>
        public static readonly Point None = new(float.MaxValue, float.MaxValue);
        public float X { get; }
        public float Y { get; }

        public static bool operator ==(Point left, Point right) => left.Equals(right);

        public static bool operator !=(Point left, Point right) => !(left == right);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> struct.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public bool Equals(Point other) => X.NearlyEquals(other.X, 0.1f) && Y.NearlyEquals(other.Y, 0.1f);

        public override bool Equals(object? obj) => obj is Point other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X:N0}, {Y:N0})";
    }
}