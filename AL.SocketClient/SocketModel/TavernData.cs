#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     One tavern frame. The event name is shared by the reply to an info request and by the round's own bet/won/lost
///     broadcasts, so <see cref="Event" /> is the discriminator, not the message type.
/// </summary>
public sealed record TavernData
{
    /// <summary>
    ///     <c>
    ///         up
    ///     </c>
    ///     or
    ///     <c>
    ///         down
    ///     </c>
    ///     .
    /// </summary>
    [JsonPropertyName("dir")]
    public string? Direction { get; init; }

    /// <summary>
    ///     On an
    ///     <c>
    ///         info
    ///     </c>
    ///     frame, the percentage of a win's profit the house keeps. Steps down as the bank grows - 2 at or below a billion,
    ///     then 1.5, 1 and 0.5 (node/server_functions.js:1245).
    /// </summary>
    [JsonPropertyName("edge")]
    public float Edge { get; init; }

    /// <summary>
    ///     Which frame this is:
    ///     <c>
    ///         info
    ///     </c>
    ///     is the reply to a query, and
    ///     <c>
    ///         bet
    ///     </c>
    ///     ,
    ///     <c>
    ///         won
    ///     </c>
    ///     and
    ///     <c>
    ///         lost
    ///     </c>
    ///     are broadcast to everyone in the tavern.
    /// </summary>
    /// <remarks>
    ///     Absent on one shape - the roulette handler echoes the raw bet record back to the bettor's own socket
    ///     (node/server.js:11511), and that record has a
    ///     <c>
    ///         state
    ///     </c>
    ///     where every other frame has an
    ///     <c>
    ///         event
    ///     </c>
    ///     . So this can read null: guard for it before comparing, and keep the literal on the left, because
    ///     <c>
    ///         EqualsI
    ///     </c>
    ///     throws on a null receiver.
    /// </remarks>
    [JsonPropertyName("event")]
    public string? Event { get; init; }

    /// <summary>
    ///     The stake on a
    ///     <c>
    ///         bet
    ///     </c>
    ///     or
    ///     <c>
    ///         lost
    ///     </c>
    ///     frame, and the gross win on a
    ///     <c>
    ///         won
    ///     </c>
    ///     frame.
    /// </summary>
    [JsonPropertyName("gold")]
    public long Gold { get; init; }

    /// <summary>
    ///     On an
    ///     <c>
    ///         info
    ///     </c>
    ///     frame, the largest net win the bank will cover: 40% of
    ///     <c>
    ///         S.gold - house_debt()
    ///     </c>
    ///     . The bet handler refuses anything above it (node/server.js:11540).
    /// </summary>
    /// <remarks>
    ///     Moves with every other player's open bets, not only this character's, because
    ///     <c>
    ///         house_debt
    ///     </c>
    ///     sums what the house stands to lose across the whole server.
    /// </remarks>
    [JsonPropertyName("max")]
    public long Max { get; init; }

    /// <summary>
    ///     Whose bet a
    ///     <c>
    ///         bet
    ///     </c>
    ///     ,
    ///     <c>
    ///         won
    ///     </c>
    ///     or
    ///     <c>
    ///         lost
    ///     </c>
    ///     frame is about.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    ///     Profit after the house edge and the stake, on a
    ///     <c>
    ///         won
    ///     </c>
    ///     frame only (node/server_functions.js:1347).
    /// </summary>
    [JsonPropertyName("net")]
    public long Net { get; init; }

    /// <summary>
    ///     The number that was bet on.
    /// </summary>
    [JsonPropertyName("num")]
    public float Number { get; init; }

    /// <summary>
    ///     The game the frame is about;
    ///     <c>
    ///         dice
    ///     </c>
    ///     for everything this client sends.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}