using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    /// <summary>
    ///     <inheritdoc cref="IRectangle" /> <br />
    ///     Represents a tile on a map.
    /// </summary>
    /// <seealso cref="IRectangle" />
    public record Tile : IRectangle
    {
        /// <summary>
        ///     Tiles are square. The size is the height and width of the tile.
        /// </summary>
        [JsonProperty, JsonArrayIndex(3)]
        public float Size { get; init; }

        /// <summary>
        ///     The name of the tileset this tile belongs to.
        /// </summary>
        [JsonProperty, JsonArrayIndex(0)]
        public string TileSet { get; init; } = null!;

        /// <summary>
        ///     TODO: Unknown
        /// </summary>
        [JsonProperty, JsonArrayIndex(4)]
        public float Unknown { get; init; }

        /// <summary>
        ///     The X coordinate of the top left of the tile.
        /// </summary>
        [JsonProperty, JsonArrayIndex(1)]
        public float X { get; init; }

        /// <summary>
        ///     The Y coordinate of the top left of the tile.
        /// </summary>
        [JsonProperty, JsonArrayIndex(2)]
        public float Y { get; init; }

        public float Bottom => Y + Size;

        public float Height => Size;

        public float Left => X;

        public float Right => X + Size;

        public float Top => Y;

        public IPoint[] Vertices => new IPoint[]
        {
            new Point(((IRectangle) this).Top, ((IRectangle) this).Left),
            new Point(((IRectangle) this).Top, ((IRectangle) this).Right),
            new Point(((IRectangle) this).Bottom, ((IRectangle) this).Left),
            new Point(((IRectangle) this).Bottom, ((IRectangle) this).Right)
        };

        public float Width => Size;
    }
}