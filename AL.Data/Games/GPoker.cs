#region
using System.Text.Json.Serialization;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Games;

/// <summary>
///     The tavern's Texas Hold'em table, added to the game data in version 16846 along with a <c>poker</c> machine on the
///     tavern map.
/// </summary>
/// <remarks>
///     None of the six script functions this game exposes - <c>get_poker_table</c>, <c>poker_join</c>,
///     <c>poker_leave</c>, <c>poker_act</c>, <c>poker_sit_in</c> and <c>poker_sit_out</c> - are modelled, and neither is
///     the event that carries table state. The published server source has only an empty <c>tavern.poker</c> placeholder,
///     so every payload and reply shape is unknown. This record covers the static half only.
/// </remarks>
public record GPoker
{
    /// <summary>
    ///     How long a seat has to act before the table acts for it, in milliseconds.
    /// </summary>
    [JsonPropertyName("action_ms")]
    public int ActionMS { get; init; }

    /// <summary>
    ///     The extra time a seat may draw on beyond <see cref="ActionMS" />, in milliseconds. A time bank, spent once rather
    ///     than per hand on the usual arrangement, though the rule that governs it is not published.
    /// </summary>
    [JsonPropertyName("bank_ms")]
    public int BankMS { get; init; }

    /// <summary>
    ///     The pause between one hand ending and the next being dealt, in milliseconds.
    /// </summary>
    [JsonPropertyName("between_ms")]
    public int BetweenMS { get; init; }

    /// <summary>
    ///     Published as 2. It governs how blinds are handled for a seat that has just joined or come back from sitting out;
    ///     the exact rule is not in the game data and the handler is not published.
    /// </summary>
    [JsonPropertyName("blind_hands")]
    public int BlindHands { get; init; }

    /// <summary>
    ///     The stake levels, keyed by the name a table is chosen by - <c>I</c> through <c>IV</c>, plus <c>PVP</c>. They run
    ///     from a 100,000 small blind to a 100,000,000 one.
    /// </summary>
    public IReadOnlyDictionary<string, GBlindLevel> Blinds { get; init; } = new Dictionary<string, GBlindLevel>();

    /// <summary>
    ///     The table's own footprint, as a rectangle offset from where the table stands. Carries no map name, because the
    ///     wire form is the four-coordinate one.
    /// </summary>
    public MapRectangle Block { get; init; } = null!;

    /// <summary>
    ///     The smallest and largest stack a seat accepts, in big blinds.
    /// </summary>
    [JsonPropertyName("buyin")]
    public GBuyIn BuyIn { get; init; } = null!;

    /// <summary>
    ///     How long a seat is held for a player who has left the table, in milliseconds. Five minutes, so a short walk does
    ///     not cost a stack.
    /// </summary>
    [JsonPropertyName("grace_ms")]
    public int GraceMS { get; init; }

    /// <summary>
    ///     The nine hand rankings, weakest first, from <c>high_card</c> to <c>straight_flush</c>. Ordinal position in this
    ///     list is the comparison: a later entry beats an earlier one.
    /// </summary>
    public IReadOnlyList<string> Hands { get; init; } = [];

    /// <summary>
    ///     The share of a pot the house takes, as a percentage.
    /// </summary>
    public float Rake { get; init; }

    /// <summary>
    ///     The ceiling on <see cref="Rake" /> for one pot. Published as 10; the unit is not stated in the game data, and the
    ///     handler that applies it is not published.
    /// </summary>
    [JsonPropertyName("rake_cap")]
    public int RakeCap { get; init; }

    /// <summary>
    ///     The thirteen card ranks, lowest first. Ordinal position is the comparison, the same as <see cref="Hands" />.
    /// </summary>
    public IReadOnlyList<string> Ranks { get; init; } = [];

    /// <summary>
    ///     How close a character has to stand to the table to take a seat.
    /// </summary>
    public float Reach { get; init; }

    /// <summary>
    ///     How many players the table holds.
    /// </summary>
    public int Seats { get; init; }

    /// <summary>
    ///     How long the table shows a finished hand before clearing it, in milliseconds.
    /// </summary>
    [JsonPropertyName("showdown_ms")]
    public int ShowdownMS { get; init; }

    /// <summary>
    ///     Where each seat stands, as an offset from the table rather than a point on the map: three in front and two behind,
    ///     symmetric about the table. There are <see cref="Seats" /> of them, in seat order.
    /// </summary>
    public IReadOnlyList<Point> Stools { get; init; } = [];

    /// <summary>
    ///     The four suits. Unranked - no game here breaks a tie on suit.
    /// </summary>
    public IReadOnlyList<string> Suits { get; init; } = [];
}
