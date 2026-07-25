#region
using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when a game error occurs.
/// </summary>
/// <seealso cref="IOptionalObject" />
[JsonConverter(typeof(StringOrObjectConverter<GameMessageData>), nameof(Message))]
public sealed record GameMessageData : IOptionalObject
{
    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>
    ///     The game error/log message.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     If populated, the display color of a game_log line - a hex value (e.g. "#64B867") or a named color.
    ///     game_log object payloads only; game_error is always a plain string.
    /// </summary>
    [JsonProperty]
    public string? Color { get; init; }

    /// <summary>
    ///     If populated, the name of the player to shower with confetti (set only by the giveaway-win game_log).
    /// </summary>
    [JsonProperty]
    public string? Confetti { get; init; }
}