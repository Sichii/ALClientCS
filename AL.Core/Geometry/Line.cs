using System;
using System.Text.Json.Serialization;
using AL.Core.Extensions;
using AL.Core.Interfaces;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="ILine" />
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.ILine" />
    public record Line : ILine
    {
        public IPoint Point1 { get; init; }
        public IPoint Point2 { get; init; }

        /// <summary>
        ///     The euclidean distance between <see cref="Point1" /> and <see cref="Point2" />
        /// </summary>

        [JsonIgnore]
        public float Length => Point1.Distance(Point2);

        /*
        /// <summary>
        ///     Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        public Line()
        {
            Point1 = Point.None;
            Point2 = Point.None;
        }*/

        /// <summary>
        ///     Initializes a new instance of the <see cref="Line" /> class.
        /// </summary>
        /// <param name="point1">A point</param>
        /// <param name="point2">Another point.</param>
        /// <exception cref="System.ArgumentNullException">point1</exception>
        /// <exception cref="System.ArgumentNullException">point2</exception>
        public Line(IPoint point1, IPoint point2)
        {
            Point1 = point1 ?? throw new ArgumentNullException(nameof(point1));
            Point2 = point2 ?? throw new ArgumentNullException(nameof(point2));
        }

        public void Deconstruct(out IPoint point1, out IPoint point2)
        {
            point1 = Point1;
            point2 = Point2;
        }
    }
}