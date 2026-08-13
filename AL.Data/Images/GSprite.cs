#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     One character or monster sheet. <see cref="Matrix" /> says which skin sits in which cell of a
///     <see cref="Rows" /> by <see cref="Columns" /> grid.
/// </summary>
/// <remarks>
///     Every cell holds the same animation - three frames across by four facings down - so a cell is twelve frames and
///     a still image is the middle frame of the first facing. A sheet carrying a <c>type</c> is a cosmetic laid out
///     differently (a hat is one frame across, a tail is four); no monster is on one of those, so the type is not
///     bound.
/// </remarks>
public sealed record GSprite
{
    /// <summary>How many skins across the sheet is.</summary>
    public int Columns { get; init; }

    /// <summary>The path the sheet is served from, which may carry a cache-busting query.</summary>
    public string File { get; init; } = string.Empty;

    /// <summary>Row-major, one skin name per cell. A null cell is grid the sheet does not use.</summary>
    public IReadOnlyList<IReadOnlyList<string?>> Matrix { get; init; } = [];

    /// <summary>How many skins down the sheet is.</summary>
    public int Rows { get; init; }

    /// <summary>
    ///     Whether the game leaves this sheet out of its own skin lookup, which makes every name in
    ///     <see cref="Matrix" /> unreachable through it.
    /// </summary>
    public bool Skip { get; init; }

    /// <summary>The <see cref="GameData.Images" /> key for this sheet: the path with any query cut off.</summary>
    [JsonIgnore]
    public string ImageKey => File.Split('?')[0];
}
