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
        private int End;
        /// <summary>
        ///     The X or Y coordinate the line exists on.
        /// </summary>
        [JsonProperty, JsonArrayIndex(0)]
        private int On;
        /// <summary>
        ///     The X or Y coordinate the line starts on.
        /// </summary>
        [JsonProperty, JsonArrayIndex(1)]
        private int Start;
        /// <summary>
        ///     Whether or not this line exists on a single X coordinate.
        /// </summary>
        /// <value><c>true</c> if <see cref="On" /> represents an X coordinate; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        internal bool IsX { get; init; }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public float Length => Math.Abs(Start - End);

        public IPoint Point1 => IsX ? new Point(On, Start) : new Point(Start, On);
        public IPoint Point2 => IsX ? new Point(On, End) : new Point(End, On);

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

            if (IsX != other.IsX)
                throw new ArgumentException($"{nameof(IsX)} and {nameof(other)}.{nameof(IsX)} must be equal.");

            var edges = new[] { Start, End, other.Start, other.End };

            return new StraightLine
            {
                On = On,
                Start = edges.Min(),
                End = edges.Max(),
                IsX = IsX
            };
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

            if (IsX != other.IsX)
                throw new ArgumentException($"{nameof(IsX)} and {nameof(other)}.{nameof(IsX)} must be equal.");

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

            if (IsX == other.IsX)
                throw new ArgumentException($"{nameof(IsX)} and {nameof(other)}.{nameof(IsX)} must NOT be equal.");

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