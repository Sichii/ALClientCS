using System;
using System.Collections;
using System.Collections.Generic;
using AL.Core.Interfaces;

// ReSharper disable ConstantConditionalAccessQualifier

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IRectangle" /> <br />
    ///     The rectangle is defined by the measurements of a <see cref="BoundingBase" /> and an <see cref="IPoint" />.
    /// </summary>
    /// <seealso cref="BoundingBase" />
    /// <seealso cref="AL.Core.Interfaces.IRectangle" />
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record BoundingRectangle : IBounding, IRectangle
    {
        private readonly IPoint Center;
        public float Bottom => Y + VerticalNotNorth;
        public float HalfWidth { get; }
        public float Height => VerticalNorth + VerticalNotNorth;
        public float Left => X - HalfWidth;
        public float Right => X + HalfWidth;
        public float Top => Y - VerticalNorth;
        public float VerticalNorth { get; }
        public float VerticalNotNorth { get; }
        public IReadOnlyList<IPoint> Vertices => new IPoint[]
            { new Point(Left, Top), new Point(Right, Top), new Point(Right, Bottom), new Point(Left, Bottom) };
        public float Width => HalfWidth * 2;
        public float X => Center.X;
        public float Y => Center.Y;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoundingRectangle" /> class.
        /// </summary>
        /// <param name="x">The x coordinate of the center of the bounding box.</param>
        /// <param name="y">The y coordinate of the center of the bounding box.</param>
        /// <param name="halfWidth">Half of the width of the bounding box.</param>
        /// <param name="verticalNorth">The distance between the center and top of the bounding box.</param>
        /// <param name="verticalNotNorth">The distance between the center and bottom of the bounding box.</param>
        public BoundingRectangle(
            float x,
            float y,
            float halfWidth,
            float verticalNorth,
            float verticalNotNorth)
            : this(new Point(x, y), new BoundingBase(halfWidth, verticalNorth, verticalNotNorth)) { }
        
        // ReSharper disable once UseDeconstructionOnParameter        
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoundingRectangle" /> class.
        /// </summary>
        /// <param name="center">The center point of the bounding box.</param>
        /// <param name="boundingBase">The measurements of the bounding box.</param>
        /// <exception cref="System.ArgumentNullException">center</exception>
        /// <exception cref="System.ArgumentNullException">boundingBase</exception>
        public BoundingRectangle(IPoint center, BoundingBase boundingBase)
        {
            Center = center ?? throw new ArgumentNullException(nameof(center));

            if (boundingBase == null)
                throw new ArgumentNullException(nameof(boundingBase));

            HalfWidth = boundingBase.HalfWidth;
            VerticalNorth = boundingBase.VerticalNorth;
            VerticalNotNorth = boundingBase.VerticalNotNorth;
        }

        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}