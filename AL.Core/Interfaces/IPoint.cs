using System;
using System.Collections.Generic;
using AL.Core.Comparers;

namespace AL.Core.Interfaces
{
    /// <summary>
    ///     Represents a coordinate pair.
    /// </summary>
    public interface IPoint : IEquatable<IPoint>
    {
        static IEqualityComparer<IPoint> Comparer { get; } = new PointEqualityComparer();

        /// <summary>
        ///     An X coordinate.
        /// </summary>
        float X { get; }

        /// <summary>
        ///     A Y coordinate.
        /// </summary>
        float Y { get; }

        int GetHashCode() => HashCode.Combine(Convert.ToInt32(X), Convert.ToInt32(Y));
        static string ToString(IPoint point) => $"({Convert.ToInt32(point.X):N0}, {Convert.ToInt32(point.Y):N0})";
    }
}