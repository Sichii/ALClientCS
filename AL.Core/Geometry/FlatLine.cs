using System;
using System.Collections.Generic;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;

namespace AL.Core.Geometry
{
    public record FlatLine : ILine
    {
        [JsonProperty, JsonArrayIndex(2)]
        private int End;
        [JsonProperty, JsonArrayIndex(0)]
        private int On;
        [JsonProperty, JsonArrayIndex(1)]
        private int Start;
        [JsonIgnore]
        internal bool IsX { get; init; }
        public float Length => Math.Abs(Start - End);

        public IPoint Point1 => IsX ? new Point(On, Start) : new Point(Start, On);
        public IPoint Point2 => IsX ? new Point(On, End) : new Point(End, On);

        public int MaxDistance(FlatLine other) => Math.Max(End, other.End) - Math.Min(Start, other.Start);

        public FlatLine Merge(FlatLine other) => new()
            { On = On, Start = Math.Min(Start, other.Start), End = Math.Max(End, other.End) };

        public bool Overlaps(FlatLine other) => On == other.On && MaxDistance(other) < Length + other.Length;

        public static bool PerpindicularIntersects(FlatLine line, FlatLine other) =>
            other.Start <= line.On && line.On <= other.End && line.Start <= other.On && other.On <= line.End;

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