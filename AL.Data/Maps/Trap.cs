using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    /// <summary>
    ///     Represents the static data for a trap on a map.
    /// </summary>
    public record Trap
    {
        /// <summary>
        ///     The coordinate of the trap.
        /// </summary>
        [JsonConverter(typeof(ArrayToPointConverter))]
        public Point Position { get; init; }

        /// <summary>
        ///     The type of trap.
        /// </summary>
        public TrapType Type { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this is the vertices of the polygon of the trap.<br />
        ///     TODO: Add IPolygon support
        /// </summary>
        [JsonProperty("polygon", ItemConverterType = typeof(ArrayToPointConverter))]
        public IReadOnlyList<Point>? Vertices { get; init; }
    }
}