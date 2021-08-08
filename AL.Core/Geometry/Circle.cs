using System;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     Represents a circle.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Circle(float X, float Y, float Radius) : ICircle
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Circle" /> class.
        /// </summary>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <exception cref="System.ArgumentNullException">center</exception>
        public Circle(IPoint center, float radius)
            : this(center.X, center.Y, radius)
        {
            if (center == null)
                throw new ArgumentNullException(nameof(center));
        }

        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ICircle? other) => ICircle.Comparer.Equals(this, other);

        public override int GetHashCode() => ICircle.Comparer.GetHashCode(this);
    }
}