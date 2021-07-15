﻿using AL.Core.Definitions;
using AL.Core.Geometry;
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
        ///     A polygon representing the bounds of the zone. <br />
        /// </summary>
        [JsonProperty("polygon", ItemConverterType = typeof(ArrayToPointConverter))]
        public Polygon Vertices { get; init; } = null!;
    }
}