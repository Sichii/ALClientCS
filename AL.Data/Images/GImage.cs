namespace AL.Data.Images;

/// <summary>
///     The pixel size of one of the game's asset files, keyed in <see cref="GameData.Images" /> by its path with no
///     cache-busting query on it.
/// </summary>
/// <remarks>
///     A sheet carries the grid it is cut into but not the size of the file that grid covers, so this is what turns a
///     row and a column into pixels. Only the character and monster sheets need it - an item sheet states its own
///     icon size.
/// </remarks>
public sealed record GImage
{
    /// <summary>The file's height in pixels.</summary>
    public int Height { get; init; }

    /// <summary>The file's format, as the game writes it. Everything currently shipped is <c>png</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The file's width in pixels.</summary>
    public int Width { get; init; }
}
