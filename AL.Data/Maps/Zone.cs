using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    /// <summary>
    ///     Represents a special zone on a map.
    /// </summary>
    public record Zone
    {
        /// <summary>
        ///     The type of drop this zone yields.
        /// </summary>
        public DropType Drop { get; init; }

        /// <summary>
        ///     The type of zone.
        /// </summary>
        public ZoneType Type { get; init; }

        /// <summary>
        ///     Vertices of the polygon of the zone. <br />
        ///     TODO: Add IPolygon support
        /// </summary>
        [JsonProperty("polygon", ItemConverterType = typeof(ArrayToPointConverter))]
        public IPoint[] Vertices { get; init; } = null!;
    }
}