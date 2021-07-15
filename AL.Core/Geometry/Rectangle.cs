using System;
using System.Collections;
using System.Collections.Generic;
using AL.Core.Interfaces;
using Newtonsoft.Json;

// ReSharper disable ConstantConditionalAccessQualifier

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IRectangle" />
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IRectangle" />
    public record Rectangle : IRectangle
    {
        public float Bottom { get; }
        public float Height { get; }
        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public IReadOnlyList<IPoint> Vertices { get; }
        public float Width { get; }
        public float X { get; }
        public float Y { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rectangle" /> class.
        /// </summary>
        /// <param name="x">The center x coordinate.</param>
        /// <param name="y">The center y coordinate.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        [JsonConstructor]
        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            Top = y - height / 2;
            Left = x - width / 2;
            Right = x + width / 2;
            Bottom = y + height / 2;

            Vertices = new IPoint[]
                { new Point(Left, Top), new Point(Right, Top), new Point(Right, Bottom), new Point(Left, Bottom) };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rectangle" /> class.
        /// </summary>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="center">The center point of the rectangle.</param>
        /// <exception cref="System.ArgumentNullException">center</exception>
        public Rectangle(float height, float width, IPoint center)
            : this(center?.X ?? throw new ArgumentNullException(nameof(center)), center.Y, width, height) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rectangle" /> class.
        /// </summary>
        /// <param name="pt1">An opposing vertex of a rectangle.</param>
        /// <param name="pt2">Another opposing vertex of a rectangle.</param>
        /// <exception cref="System.ArgumentNullException">pt1</exception>
        /// <exception cref="System.ArgumentNullException">pt2</exception>
        public Rectangle(IPoint pt1, IPoint pt2)
            : this(
                Math.Abs((pt1?.Y ?? throw new ArgumentNullException(nameof(pt1)))
                         - (pt2?.Y ?? throw new ArgumentNullException(nameof(pt2)))), Math.Abs(pt1.X - pt2.X),
                new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2)) { }

        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}