namespace AL.Data.Images;

/// <summary>
///     Represents the pixel size of one of the game's asset files, keyed in <see cref="GameData.Images" /> by its path with
///     no cache-busting query on it.
/// </summary>
public sealed record GImage
{
    /// <summary>The file's height in pixels.</summary>
    public int Height { get; init; }

    /// <summary>
    ///     The file's format, such as <c>png</c>.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The file's width in pixels.</summary>
    public int Width { get; init; }
}