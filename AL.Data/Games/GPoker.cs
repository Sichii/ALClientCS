#region
using System.Text.Json.Serialization;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Games;

/// <summary>Represents the tavern's Texas Hold'em table.</summary>
/// <remarks>
///     The script functions and socket events that play it are not modelled; this record covers the static data only.
/// </remarks>
public record GPoker
{
    /// <summary>
    ///     How long a seat has to act before the table acts for it, in milliseconds.
    /// </summary>
    [JsonPropertyName("action_ms")]
    public int ActionMS { get; init; }

    /// <summary>
    ///     The extra time a seat may draw on beyond <see cref="ActionMS" />, in milliseconds.
    /// </summary>
    [JsonPropertyName("bank_ms")]
    public int BankMS { get; init; }

    /// <summary>
    ///     The pause between one hand ending and the next being dealt, in milliseconds.
    /// </summary>
    [JsonPropertyName("between_ms")]
    public int BetweenMS { get; init; }

    /// <summary>
    ///     The blind rule for a seat that has just joined or come back from sitting out. Its exact meaning is not published.
    /// </summary>
    [JsonPropertyName("blind_hands")]
    public int BlindHands { get; init; }

    /// <summary>
    ///     The stake levels, keyed by the name a table is chosen by: <c>I</c> through <c>IV</c>, plus <c>PVP</c>.
    /// </summary>
    public IReadOnlyDictionary<string, GBlindLevel> Blinds { get; init; } = new Dictionary<string, GBlindLevel>();

    /// <summary>
    ///     The table's own footprint, as a rectangle offset from where the table stands. Carries no map name.
    /// </summary>
    public MapRectangle Block { get; init; } = null!;

    /// <summary>
    ///     The smallest and largest stack a seat accepts, in big blinds.
    /// </summary>
    [JsonPropertyName("buyin")]
    public GBuyIn BuyIn { get; init; } = null!;

    /// <summary>
    ///     How long a seat is held for a player who has left the table, in milliseconds.
    /// </summary>
    [JsonPropertyName("grace_ms")]
    public int GraceMS { get; init; }

    /// <summary>
    ///     The hand rankings, weakest first, from <c>high_card</c> to <c>straight_flush</c>.
    /// </summary>
    public IReadOnlyList<string> Hands { get; init; } = [];

    /// <summary>
    ///     The share of a pot the house takes, as a percentage.
    /// </summary>
    public float Rake { get; init; }

    /// <summary>
    ///     The ceiling on <see cref="Rake" /> for one pot. Its unit is not published.
    /// </summary>
    [JsonPropertyName("rake_cap")]
    public int RakeCap { get; init; }

    /// <summary>The card ranks, lowest first.</summary>
    public IReadOnlyList<string> Ranks { get; init; } = [];

    /// <summary>
    ///     How close a character has to stand to the table to take a seat.
    /// </summary>
    public float Reach { get; init; }

    /// <summary>How many players the table holds.</summary>
    public int Seats { get; init; }

    /// <summary>
    ///     How long the table shows a finished hand before clearing it, in milliseconds.
    /// </summary>
    [JsonPropertyName("showdown_ms")]
    public int ShowdownMS { get; init; }

    /// <summary>
    ///     Where each seat stands, as an offset from the table, in seat order.
    /// </summary>
    public IReadOnlyList<Point> Stools { get; init; } = [];

    /// <summary>The suits, which are unranked.</summary>
    public IReadOnlyList<string> Suits { get; init; } = [];
}