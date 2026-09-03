namespace AL.SocketClient.Model;

/// <summary>
///     The cosmetics a <see cref="Player" /> is currently wearing, one name per slot.
/// </summary>
/// <remarks>
///     Nine of the ten slots are the non-<c>skin</c> values of the server's <c>cxtype_to_slot</c> map
///     (js/old_common_functions.js:135). <c>upper</c> is the tenth and appears in no such map: the server writes it
///     from a branch of its own (node/server.js:4849), and only for a body or armor sprite. A body, armor or
///     character sprite sent to the <c>skin</c> slot writes <c>player.skin</c> instead of anything here, and arrives
///     as <see cref="Player.Skin" />.
///     <br />
///     Every member is nullable because an empty slot sends no key at all rather than an empty string.
/// </remarks>
public sealed class CosmeticInfo
{
    /// <summary>Wings. Clearing this slot clears <see cref="Tail" /> along with it.</summary>
    public string? Back { get; init; }

    /// <summary>A beard or a mask.</summary>
    public string? Chin { get; init; }

    /// <summary>Clearing this slot clears <see cref="Makeup" /> along with it.</summary>
    public string? Face { get; init; }

    /// <summary>Shown in place of the default gravestone when the character dies.</summary>
    public string? Gravestone { get; init; }

    public string? Hair { get; init; }

    public string? Hat { get; init; }

    public string? Head { get; init; }

    public string? Makeup { get; init; }

    /// <summary>Cleared as a side effect whenever <see cref="Back" /> is cleared.</summary>
    public string? Tail { get; init; }

    /// <summary>A body or armor sprite worn over the base skin.</summary>
    public string? Upper { get; init; }
}