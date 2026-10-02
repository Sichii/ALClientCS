#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents one tavern frame. The info reply and the bet, won and lost broadcasts share one event name, so
///     <see cref="Event" /> tells them apart.
/// </summary>
public sealed record TavernData
{
    /// <summary>The bet's direction, <c>up</c> or <c>down</c>.</summary>
    [JsonPropertyName("dir")]
    public string? Direction { get; init; }

    /// <summary>
    ///     On an <c>info</c> frame, the percentage of a win's profit the house keeps. Steps down as the bank grows: 2 at or
    ///     below a billion, then 1.5, 1 and 0.5.
    /// </summary>
    [JsonPropertyName("edge")]
    public float Edge { get; init; }

    /// <summary>
    ///     The kind of frame: <c>info</c> is the reply to a query, and <c>bet</c>, <c>won</c> and <c>lost</c> are broadcast to
    ///     everyone in the tavern.
    /// </summary>
    /// <remarks>
    ///     Null on the raw bet record the roulette handler echoes to the bettor, which has a <c>state</c> instead. Keep the
    ///     literal on the left when comparing.
    /// </remarks>
    [JsonPropertyName("event")]
    public string? Event { get; init; }

    /// <summary>
    ///     The stake on a <c>bet</c> or <c>lost</c> frame, and the gross win on a <c>won</c> frame.
    /// </summary>
    [JsonPropertyName("gold")]
    public long Gold { get; init; }

    /// <summary>
    ///     On an <c>info</c> frame, the largest net win the bank will cover: 40% of <c>S.gold - house_debt()</c>. The bet
    ///     handler refuses anything above it.
    /// </summary>
    /// <remarks>
    ///     Moves with every player's open bets, because <c>house_debt</c> sums what the house stands to lose server-wide.
    /// </remarks>
    [JsonPropertyName("max")]
    public long Max { get; init; }

    /// <summary>
    ///     The bettor a <c>bet</c>, <c>won</c> or <c>lost</c> frame is about.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    ///     Profit after the house edge and the stake, on a <c>won</c> frame only.
    /// </summary>
    [JsonPropertyName("net")]
    public long Net { get; init; }

    /// <summary>The number that was bet on.</summary>
    [JsonPropertyName("num")]
    public float Number { get; init; }

    /// <summary>
    ///     The game the frame is about; <c>dice</c> for everything this client sends.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}