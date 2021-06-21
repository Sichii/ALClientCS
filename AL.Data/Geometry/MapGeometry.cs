using System;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    public record MapGeometry : IRectangle
    {
        [JsonProperty("max_x")]
        public int MaxX { get; init; }

        [JsonProperty("max_y")]
        public int MaxY { get; init; }

        [JsonProperty("min_x")]
        public int MinX { get; init; }

        [JsonProperty("min_y")]
        public int MinY { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<Tile>))]
        public Tile[] Tiles { get; init; }

        [JsonProperty("x_lines", ItemConverterType = typeof(ArrayToObjectConverter<FlatLine>))]
        public FlatLine[] XLines { get; internal set; }

        [JsonProperty("y_lines", ItemConverterType = typeof(ArrayToObjectConverter<FlatLine>))]
        public FlatLine[] YLines { get; internal set; }

        float IRectangle.Bottom => MaxY;

        float IRectangle.Height => MaxY - MinY;

        float IRectangle.Left => MinX;

        string IRectangle.Map => throw new NotImplementedException();

        float IRectangle.Right => MaxX;

        float IRectangle.Top => MinY;

        IPoint[] IRectangle.Vertices => new IPoint[]
        {
            new Point(((IRectangle) this).Top, ((IRectangle) this).Left),
            new Point(((IRectangle) this).Top, ((IRectangle) this).Right),
            new Point(((IRectangle) this).Bottom, ((IRectangle) this).Left),
            new Point(((IRectangle) this).Bottom, ((IRectangle) this).Right)
        };

        float IRectangle.Width => MaxX - MinX;

        //public object Points { get; set; }
        //public object Rectangles { get; set; }
        //public object Polygons { get; set; }
        //public int[][] Placements { get; set; }
        //public int Default { get; set; }
        float IPoint.X => 0;

        float IPoint.Y => 0;
    }
}