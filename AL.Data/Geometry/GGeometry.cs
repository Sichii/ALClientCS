﻿using System.Collections;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IRectangle" /> <br />
    ///     Provides geometric information for a map.
    /// </summary>
    /// <seealso cref="IRectangle" />
    public record GGeometry : IRectangle
    {
        /// <summary>
        ///     Maximum X coordinate on the map.
        /// </summary>
        [JsonProperty("max_x")]
        public int MaxX { get; init; }

        /// <summary>
        ///     Maximum Y coordinate on the map.
        /// </summary>
        [JsonProperty("max_y")]
        public int MaxY { get; init; }

        /// <summary>
        ///     Minimum X coordinate on the map.
        /// </summary>
        [JsonProperty("min_x")]
        public int MinX { get; init; }

        /// <summary>
        ///     Minimum Y coordinate on the map.
        /// </summary>
        [JsonProperty("min_y")]
        public int MinY { get; init; }

        /// <summary>
        ///     A list of tiles on this map.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<GTile>))]
        public IReadOnlyList<GTile> Tiles { get; init; } = new List<GTile>();

        /// <summary>
        ///     A list of vertical lines that should be considered as walls.
        /// </summary>
        [JsonProperty("x_lines", ItemConverterType = typeof(ArrayToObjectConverter<StraightLine>))]
        public IReadOnlyList<StraightLine> XLines { get; internal set; } = new List<StraightLine>();

        /// <summary>
        ///     A list of horizontal lines that should be considered as walls.
        /// </summary>
        [JsonProperty("y_lines", ItemConverterType = typeof(ArrayToObjectConverter<StraightLine>))]
        public IReadOnlyList<StraightLine> YLines { get; internal set; } = new List<StraightLine>();

        public float Bottom => MaxY;

        public float Height => MaxY - MinY;

        public float Left => MinX;

        public float Right => MaxX;

        public float Top => MinY;

        public IReadOnlyList<IPoint> Vertices => new IPoint[]
        {
            new Point(((IRectangle) this).Left, ((IRectangle) this).Top),
            new Point(((IRectangle) this).Right, ((IRectangle) this).Top),
            new Point(((IRectangle) this).Right, ((IRectangle) this).Bottom),
            new Point(((IRectangle) this).Left, ((IRectangle) this).Bottom)
        };

        public float Width => MaxX - MinX;

        //public object Points { get; set; }
        //public object Rectangles { get; set; }
        //public object Polygons { get; set; }
        //public int[][] Placements { get; set; }
        //public int Default { get; set; }
        public float X => (MaxX + Width) / 2;

        public float Y => (MaxY + Height) / 2;
        public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}