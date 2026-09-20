#region
using System.Text.Json;
using AL.Core.Json;
using AL.Data.Games;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The tavern's three machines ride games.wheel, games.slots and games.poker. All three are positional on the wire in
///     places, so what these pin is which array slot means what - the sort of mistake that binds cleanly and reads wrong
///     for as long as nobody checks the numbers.
/// </summary>
/// <remarks>
///     The payloads are the live shapes at game data version 16846, not the frozen data.json fixture, which predates the
///     rewrite and still carries the single-jackpot slots and the gold-prize wheel.
/// </remarks>
public class GamesTableTests
{
    private const string POKER = """
                                 {
                                   "seats": 5,
                                   "blinds": { "I": [100000, 200000], "PVP": [100000000, 200000000] },
                                   "buyin": [40, 200],
                                   "rake": 2,
                                   "rake_cap": 10,
                                   "action_ms": 20000,
                                   "bank_ms": 30000,
                                   "grace_ms": 300000,
                                   "showdown_ms": 10000,
                                   "between_ms": 2500,
                                   "blind_hands": 2,
                                   "stools": [[-32, 16], [0, 16], [32, 16], [-60, -28], [60, -28]],
                                   "reach": 40,
                                   "block": [-48, -48, 48, -1],
                                   "ranks": ["2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"],
                                   "suits": ["hearts", "diamonds", "clubs", "spades"],
                                   "hands": ["high_card", "pair", "two_pair", "three_of_a_kind", "straight", "flush", "full_house", "four_of_a_kind", "straight_flush"]
                                 }
                                 """;

    private const string SLOTS = """
                                 {
                                   "gold": 1000000,
                                   "spin": 3600,
                                   "draws": 30000,
                                   "prizes": [
                                     ["glitch", 1000000000, 6],
                                     ["goldingot", 100000000, 60],
                                     ["gem0", 20000000, 300],
                                     ["seashell", 6000000, 750],
                                     ["whiskey", 3000000, 900],
                                     ["wine", 2000000, 1200],
                                     ["ale", 1200000, 2000]
                                   ],
                                   "reels": [["ale", "wine"], ["wine", "ale"], ["ale", "seashell"]]
                                 }
                                 """;

    private const string WHEEL = """
                                 {
                                   "min": 10000,
                                   "spin": 4000,
                                   "sides": ["sun", "moon"],
                                   "slices": [["indigo", "moon", "#3D34A5"], ["pink", "sun", "#FF82CE"], ["lemon", "sun", "#FFE737"]]
                                 }
                                 """;

    /// <summary>
    ///     A prize's three slots are the item, what it pays in gold, and how many draws land on it. Payout and weight are both
    ///     bare numbers, so a swap binds cleanly and inverts the table - the rarest outcome becomes the most common.
    /// </summary>
    [Test]
    public void ASlotsPrizeBindsItsItemPayoutAndWeightInOrder()
    {
        var prize = Slots()
            .Prizes[0];

        prize.Item
             .Should()
             .Be("glitch");

        prize.Payout
             .Should()
             .Be(1_000_000_000);

        prize.Weight
             .Should()
             .Be(6);
    }

    /// <summary>
    ///     A slice's three slots are its own name, the side it pays and a display colour. The first and third are both colour
    ///     words, so a swap binds without complaint and silently makes every slice pay the same side.
    /// </summary>
    [Test]
    public void AWheelSliceBindsItsNameSideAndColourInOrder()
    {
        var slice = Wheel()
            .Slices[0];

        slice.Name
             .Should()
             .Be("indigo");

        slice.Side
             .Should()
             .Be("moon");

        slice.Colour
             .Should()
             .Be("#3D34A5");
    }

    private static GPoker Poker() => JsonSerializer.Deserialize<GPoker>(POKER, ALJson.Options)!;

    private static GSlots Slots() => JsonSerializer.Deserialize<GSlots>(SLOTS, ALJson.Options)!;

    /// <summary>
    ///     Hand rankings and card ranks are both compared by position rather than by any value in the entry, so the order they
    ///     arrive in is the whole of the rule.
    /// </summary>
    [Test]
    public void ThePokerRankingsAreOrderedWeakestFirst()
    {
        var poker = Poker();

        poker.Hands
             .Should()
             .HaveCount(9);

        poker.Hands
             .First()
             .Should()
             .Be("high_card");

        poker.Hands
             .Last()
             .Should()
             .Be("straight_flush");

        poker.Ranks
             .First()
             .Should()
             .Be("2");

        poker.Ranks
             .Last()
             .Should()
             .Be("A");

        poker.Suits
             .Should()
             .HaveCount(4);
    }

    /// <summary>
    ///     The stake levels, the buy-in bounds and the seat geometry are each a positional array inside the table.
    /// </summary>
    [Test]
    public void ThePokerTableBindsItsNestedShapes()
    {
        var poker = Poker();

        poker.Blinds["I"]
             .Small
             .Should()
             .Be(100000);

        poker.Blinds["I"]
             .Big
             .Should()
             .Be(200000);

        poker.BuyIn
             .Min
             .Should()
             .Be(40);

        poker.BuyIn
             .Max
             .Should()
             .Be(200);

        //one stool per seat, as offsets from the table rather than points on the map
        poker.Stools
             .Should()
             .HaveCount(poker.Seats);

        poker.Stools[0]
             .X
             .Should()
             .Be(-32);

        poker.Stools[0]
             .Y
             .Should()
             .Be(16);

        //the four-coordinate rectangle form, so the vertices are opposite corners and there is no map name
        poker.Block
             .Left
             .Should()
             .Be(-48);

        poker.Block
             .Right
             .Should()
             .Be(48);
    }

    /// <summary>
    ///     Seven of the table's keys carry an underscore, which the case-insensitive name match cannot reach on its own. Each
    ///     one that loses its attribute reads as zero rather than failing.
    /// </summary>
    [Test]
    public void ThePokerTableBindsItsUnderscoredKeys()
    {
        var poker = Poker();

        poker.ActionMS
             .Should()
             .Be(20000);

        poker.BankMS
             .Should()
             .Be(30000);

        poker.GraceMS
             .Should()
             .Be(300000);

        poker.ShowdownMS
             .Should()
             .Be(10000);

        poker.BetweenMS
             .Should()
             .Be(2500);

        poker.RakeCap
             .Should()
             .Be(10);

        poker.BlindHands
             .Should()
             .Be(2);
    }

    /// <summary>
    ///     The published table returns exactly what it takes: payout times weight, summed, is the pull price times the draw
    ///     count. Restated here independently rather than read off the records, because it is the one arithmetic check that
    ///     fails if payout and weight are ever bound to each other's slot.
    /// </summary>
    [Test]
    public void TheSlotsTableReturnsExactlyWhatAPullCosts()
    {
        var slots = Slots();

        slots.Prizes
             .Sum(prize => prize.Payout * prize.Weight)
             .Should()
             .Be(slots.Gold * slots.Draws);

        //and the win share is a minority of pulls, so the return is nearly all variance
        slots.Prizes
             .Sum(prize => prize.Weight)
             .Should()
             .BeLessThan(slots.Draws / 2);
    }

    /// <summary>
    ///     The two sides a stake can be placed on, and the stake floor beneath them.
    /// </summary>
    [Test]
    public void TheWheelBindsItsSidesAndItsMinimumStake()
    {
        var wheel = Wheel();

        wheel.Min
             .Should()
             .Be(10000);

        wheel.Spin
             .Should()
             .Be(4000);

        wheel.Sides
             .Should()
             .Equal("sun", "moon");

        //every slice names one of the two sides; a slice paying anything else would never settle
        wheel.Slices
             .Should()
             .OnlyContain(slice => wheel.Sides.Contains(slice.Side));
    }

    private static GWheel Wheel() => JsonSerializer.Deserialize<GWheel>(WHEEL, ALJson.Options)!;
}