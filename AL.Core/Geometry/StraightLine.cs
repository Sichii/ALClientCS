using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="ILine" /> (a straight line)
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.ILine" />
    public record StraightLine : ILine
    {
        /// <summary>
        ///     The X or Y coordinate the line ends on.
        /// </summary>
        [JsonProperty, JsonArrayIndex(2)]
        public int End { get; init; }
        /// <summary>
        ///     Whether or not this line is a vertical line.
        ///     Whether or not this line exists on a single X coordinate. <br />
        ///     Whether or not this line is an "X Line"
        /// </summary>
        /// <value><c>true</c> if <see cref="On" /> represents an X coordinate; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        public bool IsVertical { get; init; }
        /// <summary>
        ///     The X or Y coordinate the line exists on.
        /// </summary>
        [JsonProperty, JsonArrayIndex(0)]
        public int On { get; init; }
        /// <summary>
        ///     The X or Y coordinate the line starts on.
        /// </summary>
        [JsonProperty, JsonArrayIndex(1)]
        public int Start { get; init; }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public float Length => Math.Abs(Start - End);

        public IPoint Point1 => IsVertical ? new Point(On, Start) : new Point(Start, On);
        public IPoint Point2 => IsVertical ? new Point(On, End) : new Point(End, On);

        /// <summary>
        ///     Initializes a new instance of the <see cref="StraightLine" /> class.
        /// </summary>
        /// <param name="on">The X or Y value the line starts on. denoted by "isX"</param>
        /// <param name="start">The starting X or Y value of the line.</param>
        /// <param name="end">The ending X or Y value of the line.</param>
        /// <param name="isVertical">Whether or not the line starts "on" an X value.</param>
        public StraightLine(int on, int start, int end, bool isVertical)
        {
            On = on;
            Start = start;
            End = end;
            IsVertical = isVertical;
        }

        /// <summary>
        ///     Calculates the farthest distance between this straight line and another.
        /// </summary>
        /// <param name="other">Another straight line</param>
        /// <returns>
        ///     <see cref="int" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public int MaxDistance(StraightLine other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var edges = new[] { Start, End, other.Start, other.End };
            return edges.Max() - edges.Min();
        }

        /// <summary>
        ///     Merges this straight line with another straight line.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        ///     <see cref="StraightLine" />
        ///     <br />
        ///     A new <see cref="StraightLine" /> denoted by the minimum start value, and maximum end value.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        /// <exception cref="System.ArgumentException">On and other.On must be equal.</exception>
        /// <exception cref="System.ArgumentException">IsX and other.IsX must be equal.</exception>
        public StraightLine Merge(StraightLine other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (On != other.On)
                throw new ArgumentException($"{nameof(On)} and {nameof(other)}.{nameof(On)} must be equal.");

            if (IsVertical != other.IsVertical)
                throw new ArgumentException($"{nameof(IsVertical)} and {nameof(other)}.{nameof(IsVertical)} must be equal.");

            var edges = new[] { Start, End, other.Start, other.End };

            return new StraightLine(On, edges.Min(), edges.Max(), IsVertical);
        }

        /// <summary>
        ///     Determines whether this straight line overlaps another straight line.
        /// </summary>
        /// <param name="other">Another straight line.</param>
        /// <returns><c>true</c> if this straight line overlaps the <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        /// <exception cref="System.ArgumentException">IsX and other.IsX must be equal.</exception>
        public bool Overlaps(StraightLine other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (IsVertical != other.IsVertical)
                throw new ArgumentException($"{nameof(IsVertical)} and {nameof(other)}.{nameof(IsVertical)} must be equal.");

            return (On == other.On) && (MaxDistance(other) < Length + other.Length);
        }

        /// <summary>
        ///     Determines whether this straight line is perpindicular to another straight line and intersects it.
        /// </summary>
        /// <param name="other">Another straight line.</param>
        /// <returns>
        ///     <c>true</c> if this straight line is perpindicular to, and intersects the <paramref name="other" />,
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        /// <exception cref="System.ArgumentException">IsX and other.IsX must NOT be equal.</exception>
        public bool PerpindicularIntersects(StraightLine other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (IsVertical == other.IsVertical)
                throw new ArgumentException($"{nameof(IsVertical)} and {nameof(other)}.{nameof(IsVertical)} must NOT be equal.");

            return (other.Start <= On) && (On <= other.End) && (Start <= other.On) && (other.On <= End);
        }

        /// <summary>
        ///     Lazily generates all points on a straight line.
        /// </summary>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="IPoint" /></returns>
        public IEnumerable<IPoint> Points()
        {
            var current = Point1;
            var last = Point2;

            yield return current;

            while (!Equals(current, last))
            {
                current = current.DirectionalOffset(last.DirectionalRelationTo(current));
                yield return current;
            }
        }
    }
}