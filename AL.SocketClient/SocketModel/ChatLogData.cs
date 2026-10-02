#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound public chat line. A player's line reaches every socket on the server, the sender's included.
///     The outbound half is <c>SayAsync</c>.
/// </summary>
/// <remarks>
///     The same event carries NPC and system chatter, such as the pvp arena's kill announcements, local to the area rather
///     than server-wide. <see cref="IsPlayerChat" /> tells them apart.
/// </remarks>
public sealed record ChatLogData
{
    /// <summary>
    ///     If populated, the display color of the line - a hex value (e.g. "#418343"). Set only by a few NPC lines; player
    ///     chat carries none.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     The entity id of the speaker. A player's id for player chat, and otherwise whatever spoke - a monster id, "pvp", or
    ///     "mainframe".
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     Whether a player said this, which also makes the line server-wide rather than local.
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