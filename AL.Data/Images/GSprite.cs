#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     Represents one character or monster sheet. <see cref="Matrix" /> says which skin sits in which cell of a
///     <see cref="Rows" /> by <see cref="Columns" /> grid.
/// </summary>
/// <remarks>
///     Every cell holds three frames across by four facings down, and a still image is the middle frame of the first
///     facing. A sheet carrying a <see cref="Type" /> is a cosmetic laid out differently.
/// </remarks>
public sealed record GSprite
{
    /// <summary>The number of skins across the sheet.</summary>
    public int Columns { get; init; }

    /// <summary>
    ///     The path the sheet is served from, which may carry a cache-busting query.
    /// </summary>
    public string File { get; init; } = string.Empty;

    /// <summary>
    ///     The number of animation frames across one cell of an animated hat or makeup sheet. Zero when the sheet states
    ///     none, which the game's client reads as three.
    /// </summary>
    public int Frames { get; init; }

    /// <summary>
    ///     The skin names, row-major, one per cell. A null cell is unused.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string?>> Matrix { get; init; } = [];

    /// <summary>The number of skins down the sheet.</summary>
    public int Rows { get; init; }

    /// <summary>
    ///     If populated, the body size every name in <see cref="Matrix" /> is drawn at. The game reads a null as
    ///     <c>normal</c>.
    /// </summary>
    /// <remarks>
    ///     The size decides where a head sits on the body, and which of a head's skin sheets is drawn.
    /// </remarks>
    public string? Size { get; init; }

    /// <summary>
    ///     Whether the game leaves this sheet out of its skin lookup, which makes every name in <see cref="Matrix" />
    ///     unreachable through it.
    /// </summary>
    public bool Skip { get; init; }

    /// <summary>
    ///     If populated, the cosmetic type of every name in <see cref="Matrix" />, which resolves a cosmetic's slot.
    /// </summary>
    /// <remarks>
    ///     The server treats a missing type as <c>full</c>, which maps to no slot.
    /// </remarks>
    public string? Type { get; init; }

    /// <summary>
    ///     The <see cref="GameData.Images" /> key for this sheet: the path with any query cut off.
    /// </summary>
    [JsonIgnore]
    public string ImageKey => File.Split('?')[0];
}