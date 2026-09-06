#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     One transition of the tavern's dice round (node/server_functions.js:1260-1404). The round is a four-state loop and
///     this frame is how each step is announced.
/// </summary>
/// <remarks>
///     <b>
///         Gold moves a tick before the player is told.
///     </b>
///     Payouts are applied at the
///     <c>
///         roll
///     </c>
///     to
///     <c>
///         lock
///     </c>
///     transition, and the
///     <c>
///         tavern
///     </c>
///     frame naming the winner only arrives on the next one. Anything that both reads this and watches the character's
///     gold will double-count.
/// </remarks>
public sealed record DiceData
{
    /// <summary>
    ///     How <see cref="Hex" /> was produced.
    ///     <c>
    ///         hmac-sha256
    ///     </c>
    ///     .
    /// </summary>
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; init; }

    /// <summary>
    ///     The commit hash for the coming roll, on a
    ///     <c>
    ///         bets
    ///     </c>
    ///     frame.
    /// </summary>
    [JsonPropertyName("hex")]
    public string? Hex { get; init; }

    /// <summary>
    ///     The other half of the fairness reveal, on a
    ///     <c>
    ///         lock
    ///     </c>
    ///     frame.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    ///     The rolled number, on a
    ///     <c>
    ///         lock
    ///     </c>
    ///     frame only. Runs from 00.00 to 99.99.
    /// </summary>
    /// <remarks>
    ///     The server builds it by concatenating digits, so it arrives as a string and coerces on the way in.
    /// </remarks>
    [JsonPropertyName("num")]
    public float? Number { get; init; }

    /// <summary>
    ///     <c>
    ///         bets
    ///     </c>
    ///     while wagers are accepted,
    ///     <c>
    ///         roll
    ///     </c>
    ///     once they close, and
    ///     <c>
    ///         lock
    ///     </c>
    ///     when the number is revealed. The fourth state,
    ///     <c>
    ///         suspense
    ///     </c>
    ///     , announces itself with the win and loss broadcasts instead.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; init; } = null!;

    /// <summary>
    ///     Half of the fairness reveal, on a
    ///     <c>
    ///         lock
    ///     </c>
    ///     frame.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }
}