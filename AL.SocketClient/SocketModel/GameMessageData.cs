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
    ///     If populated, the display color of a game_log line - a hex value (e.g. "#64B867") or a named color. game_log object
    ///     payloads only; game_error is always a plain string.
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    ///     If populated, the name of the player to shower with confetti (set only by the giveaway-win game_log).
    /// </summary>
    public string? Confetti { get; init; }

    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>The game error/log message.</summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     If populated, the translation key behind <see cref="Message" /> , such as <c>server.game_log.gold</c> .
    /// </summary>
    /// <remarks>
    ///     Match on this rather than on the rendered text. The server renders <see cref="Message" /> in the account's own
    ///     language and rewords it freely between deploys; the key survives both. Absent on the frames the server still builds
    ///     by hand, so a matcher needs the text as a fallback.
    /// </remarks>
    [JsonPropertyName("phrase")]
    public string? Phrase { get; init; }

    /// <summary>
    ///     If populated, the values substituted into <see cref="Phrase" /> - an amount, an item name, a character name.
    /// </summary>
    /// <remarks>
    ///     A value is usually a string, but the server nests a whole phrase reference of its own wherever the substitution
    ///     is itself translated, as in <c>{"npc": "Ilex", "rival": {"phrase": "server.cave.rival"}}</c> . Typed loosely
    ///     because narrowing it to strings drops the entire frame the first time one of those arrives.
    /// </remarks>
    [JsonPropertyName("phrase_args")]
    public JsonObject? PhraseArgs { get; init; }
}