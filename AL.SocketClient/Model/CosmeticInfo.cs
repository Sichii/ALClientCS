namespace AL.SocketClient.Model;

/// <summary>
///     The cosmetics a <see cref="Player" /> is currently wearing, one name per slot.
/// </summary>
/// <remarks>
///     <c>upper</c> is written only for a body or armor sprite. A sprite sent to the <c>skin</c> slot arrives as
///     <see cref="Player.Skin" /> instead. An empty slot sends no key at all.
/// </remarks>
public sealed class CosmeticInfo
{
    /// <summary>
    ///     Wings. Clearing this slot clears <see cref="Tail" /> along with it.
    /// </summary>
    public string? Back { get; init; }

    /// <summary>A beard or a mask.</summary>
    public string? Chin { get; init; }

    /// <summary>
    ///     Clearing this slot clears <see cref="Makeup" /> along with it.
    /// </summary>
    public string? Face { get; init; }

    /// <summary>
    ///     Shown in place of the default gravestone when the character dies.
    /// </summary>
    public string? Gravestone { get; init; }

    public string? Hair { get; init; }

    public string? Hat { get; init; }

    public string? Head { get; init; }

    public string? Makeup { get; init; }

    /// <summary>
    ///     Cleared as a side effect whenever <see cref="Back" /> is cleared.
    /// </summary>
    public string? Tail { get; init; }

    /// <summary>A body or armor sprite worn over the base skin.</summary>
    public string? Upper { get; init; }
}