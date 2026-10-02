#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     Represents one item sheet: a grid of square icons, all the same size. A <see cref="GSpritePosition" /> names a cell
///     in one of these.
/// </summary>
public sealed record GImageSet
{
    /// <summary>The number of icons across the sheet.</summary>
    public int Columns { get; init; }

    /// <summary>
    ///     The path the sheet is served from, which may carry a cache-busting query.
    /// </summary>
    public string File { get; init; } = string.Empty;

    /// <summary>The number of icons down the sheet.</summary>
    public int Rows { get; init; }

    /// <summary>The side length of one icon, in pixels.</summary>
    public int Size { get; init; }

    /// <summary>
    ///     The <see cref="GameData.Images" /> key for this sheet: the path with any query cut off.
    /// </summary>
    [JsonIgnore]
    public string ImageKey => File.Split('?')[0];
}