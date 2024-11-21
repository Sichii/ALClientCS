#region
using System.Collections;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Geometry;

/// <summary>
///     <inheritdoc cref="IRectangle" />
///     <br />
///     Provides geometric information for a map.
/// </summary>
/// <seealso cref="IRectangle" />
public record GGeometry : IRectangle
{
    /// <summary>
    ///     The unique accessor for this geometry object.
    /// </summary>
    [JsonIgnore]
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     A list of horizontal lines that should be considered as walls.
    /// </summary>
    [JsonProperty("y_lines", ItemConverterType = typeof(ArrayToObjectConverter<StraightLine>))]
    public IReadOnlyList<StraightLine> HorizontalLines { get; internal set; } = new List<StraightLine>();

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
    public IReadOnlyList<StraightLine> VerticalLines { get; internal set; } = new List<StraightLine>();

    [JsonIgnore]
    public float Bottom => MaxY;

    [JsonIgnore]
    public float Height => MaxY - MinY;

    [JsonIgnore]
    public float Left => MinX;

    [JsonIgnore]
    public float Right => MaxX;

    [JsonIgnore]
    public float Top => MinY;

    [JsonIgnore]
    public IReadOnlyList<IPoint> Vertices
        =>
        [
            new Point(Left, Top),
            new Point(Right, Top),
            new Point(Right, Bottom),
            new Point(Left, Bottom)
        ];

    [JsonIgnore]
    public float Width => MaxX - MinX;

    //public object Points { get; set; }
    //public object Rectangles { get; set; }
    //public object Polygons { get; set; }
    //public int[][] Placements { get; set; }
    //public int Default { get; set; }

    [JsonIgnore]
    public float X => (MaxX + Width) / 2;

    [JsonIgnore]
    public float Y => (MaxY + Height) / 2;

    public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    public IEnumerator<IPoint> GetEnumerator() => Vertices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}