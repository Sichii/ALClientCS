#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound public chat line. A player's own chat goes out through <c>broadcast</c>, which is a bare
///     <c>io.emit</c> (node/server.js:4638, node/server_functions.js:2929) - so this reaches every socket on the server
///     rather than only the ones nearby, and the sender receives their own line back. The outbound half is
///     <c>
///         SayAsync
///     </c>
///     .
/// </summary>
/// <remarks>
///     Not every line comes from a player. The same event carries NPC and system chatter through <c>xy_emit</c> -
///     the pvp arena's kill announcements, the Grinch's phrases, cyberland's mainframe - and those are local to the
///     area rather than server-wide. <see cref="IsPlayerChat" /> is what tells them apart.
/// </remarks>
public sealed record ChatLogData
{
    /// <summary>
    ///     If populated, the display color of the line - a hex value (e.g. "#418343"). Set only by a few NPC lines;
    ///     player chat carries none.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     The entity id of the speaker. A player's id for player chat, and otherwise whatever spoke - a monster id,
    ///     "pvp", or "mainframe".
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     True when a player said this, which is also what makes the line server-wide rather than local.
    /// </summary>
    [JsonPropertyName("p")]
    public bool IsPlayerChat { get; init; }

    /// <summary>
    ///     The chat text, stripped and truncated to 1200 characters by the server.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     The display name of the speaker - a character name for player chat, an NPC or monster name otherwise.
    /// </summary>
    public string Owner { get; init; } = null!;
}
