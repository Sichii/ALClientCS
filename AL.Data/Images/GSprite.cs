#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Images;

/// <summary>
///     One character or monster sheet. <see cref="Matrix" /> says which skin sits in which cell of a <see cref="Rows" />
///     by <see cref="Columns" /> grid.
/// </summary>
/// <remarks>
///     Every cell holds the same animation - three frames across by four facings down - so a cell is twelve frames and a
///     still image is the middle frame of the first facing. A sheet carrying a <see cref="Type" /> is a cosmetic laid out
///     differently (a hat is one frame across, a tail is four); no monster is on one of those.
/// </remarks>
public sealed record GSprite
{
    /// <summary>
    ///     How many skins across the sheet is.
    /// </summary>
    public int Columns { get; init; }

    /// <summary>
    ///     The path the sheet is served from, which may carry a cache-busting query.
    /// </summary>
    public string File { get; init; } = string.Empty;

    /// <summary>
    ///     Row-major, one skin name per cell. A null cell is grid the sheet does not use.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string?>> Matrix { get; init; } = [];

    /// <summary>
    ///     How many skins down the sheet is.
    /// </summary>
    public int Rows { get; init; }

    /// <summary>
    ///     Which of the six body sizes every name in <see cref="Matrix" /> is drawn at, or null for a sheet that states none -
    ///     which the game reads as
    ///     <c>
    ///         normal
    ///     </c>
    ///     .
    /// </summary>
    /// <remarks>
    ///     What this decides is where a head sits on a body: the client shifts the head, hair and hat placements by a per-size
    ///     amount (js/html.js:5877), and picks which of a head's three skin sheets to draw from by the same key (
    ///     <c>
    ///         :5901
    ///     </c>
    ///     ). Two sizes have no skin sheet at all, so a body at one of those draws no skin layer rather than a wrongly-sized
    ///     one.
    /// </remarks>
    public string? Size { get; init; }

    /// <summary>
    ///     Whether the game leaves this sheet out of its own skin lookup, which makes every name in <see cref="Matrix" />
    ///     unreachable through it.
    /// </summary>
    public bool Skip { get; init; }

    /// <summary>
    ///     What every name in <see cref="Matrix" /> is, which is what a cosmetic's slot is resolved through. Null for a sheet
    ///     the game types as nothing - most of them.
    /// </summary>
    /// <remarks>
    ///     The server substitutes
    ///     <c>
    ///         full
    ///     </c>
    ///     for the absence while building its name-to-type table (js/old_common_functions.js:191), and
    ///     <c>
    ///         full
    ///     </c>
    ///     is a type its own
    ///     <c>
    ///         cxtype_to_slot
    ///     </c>
    ///     map has no entry for. Keeping the absence as null rather than baking that placeholder in leaves the two
    ///     distinguishable: a caller can tell a sheet nothing may be worn from off an untyped one it simply has not handled.
    /// </remarks>
    public string? Type { get; init; }

    /// <summary>
    ///     The <see cref="GameData.Images" /> key for this sheet: the path with any query cut off.
    /// </summary>
    [JsonIgnore]
    public string ImageKey => File.Split('?')[0];
}