using System;
using AL.Core.Interfaces;

// ReSharper disable ConstantConditionalAccessQualifier

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IRectangle" /> <br />
    ///     The rectangle is defined by the measurements of a <see cref="BoundingBase" /> and an <see cref="IPoint" />.
    /// </summary>
    /// <seealso cref="AL.Core.Geometry.BoundingBase" />
    /// <seealso cref="AL.Core.Interfaces.IRectangle" />
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record BoundingRectangle : BoundingBase, IRectangle
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public IPoint[] Vertices { get; }
        public float Width { get; }
        public float X { get; }
        public float Y { get; }

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
            int halfWidth,
            int verticalNorth,
            int verticalNotNorth)
            : base(halfWidth, verticalNorth, verticalNotNorth)
        {
            X = x;
            Y = y;
            Width = halfWidth * 2;
            Height = verticalNorth + verticalNotNorth;

            Top = y - verticalNorth;
            Left = x - halfWidth;
            Right = x + halfWidth;
            Bottom = y + verticalNotNorth;

            Vertices = new IPoint[]
                { new Point(Top, Left), new Point(Top, Right), new Point(Bottom, Left), new Point(Bottom, Right) };
        }

        // ReSharper disable once UseDeconstructionOnParameter        
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoundingRectangle" /> class.
        /// </summary>
        /// <param name="center">The center point of the bounding box.</param>
        /// <param name="boundingBase">The measurements of the bounding box.</param>
        /// <exception cref="System.ArgumentNullException">center</exception>
        /// <exception cref="System.ArgumentNullException">boundingBase</exception>
        public BoundingRectangle(IPoint center, BoundingBase boundingBase)
            : this(center?.X ?? throw new ArgumentNullException(nameof(center)),
                center?.Y ?? throw new ArgumentNullException(nameof(center)),
                boundingBase?.HalfWidth ?? throw new ArgumentNullException(nameof(boundingBase)),
                boundingBase?.VerticalNorth ?? throw new ArgumentNullException(nameof(boundingBase)),
                boundingBase?.VerticalNotNorth ?? throw new ArgumentNullException(nameof(boundingBase))) { }
    }
}