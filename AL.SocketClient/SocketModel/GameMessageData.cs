#region
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when a game error occurs.
/// </summary>
/// <seealso cref="IOptionalObject" />
[JsonStringOrObject(nameof(Message))]
public sealed record GameMessageData : IOptionalObject
{
    /// <summary>
    ///     If populated, the display color of a <c>game_log</c> line, a hex value (e.g. "#64B867") or a named color.
    ///     <c>game_error</c> is always a plain string.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     If populated, the name of the player to shower with confetti, set only by the giveaway-win <c>game_log</c>.
    /// </summary>
    public string? Confetti { get; init; }

    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>The game error/log message.</summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     If populated, the translation key behind <see cref="Message" />, such as <c>server.game_log.gold</c>.
    /// </summary>
    /// <remarks>
    ///     Match on this rather than the rendered text, which follows the account's language and changes between deploys.
    ///     Absent on frames the server builds by hand, so a matcher needs the text as a fallback.
    /// </remarks>
    [JsonPropertyName("phrase")]
    public string? Phrase { get; init; }

    /// <summary>
    ///     If populated, the values substituted into <see cref="Phrase" />: an amount, an item name, a character name.
    /// </summary>
    /// <remarks>
    ///     A value is usually a string, but a translated substitution nests a phrase reference of its own, as in <c>
    ///         {"npc": "Ilex", "rival": {"phrase": "server.cave.rival"}}
    ///     </c>.
    /// </remarks>
    [JsonPropertyName("phrase_args")]
    public JsonObject? PhraseArgs { get; init; }
}