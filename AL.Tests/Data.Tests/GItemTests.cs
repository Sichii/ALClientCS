#region
using AL.APIClient.Model;
using AL.Client;
using AL.Core.Definitions;
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     Which class an item names as the only one allowed to equip it - or none, for the items that stay class-neutral by
///     leaving the key out entirely.
/// </summary>
public class GItemTests : GameDataTestBed
{
    /// <summary>A single-class lock, the common case.</summary>
    [Test]
    public void AClassLockedItemNamesItsClass()
    {
        var hat = GameData.Items["mrnhat"];

        hat!.Classes
            .Should()
            .NotBeNull();

        hat.Classes!.Should()
           .BeEquivalentTo(
               new[]
               {
                   ALClass.Ranger
               });
    }

    /// <summary>
    ///     No key in the def means anybody, so this is null rather than an empty list.
    /// </summary>
    [Test]
    public void AClassNeutralItemNamesNoClass()
        => GameData.Items["cape"]!.Classes
                   .Should()
                   .BeNull();

    /// <summary>
    ///     An offer keeps a level only on an item that levels, and a quantity only on one that stacks, the way the server
    ///     reads it. A name or title the game does not have is refused outright.
    /// </summary>
    [Test]
    public void ATradeWantIsNormalizedTheServersWay()
    {
        Merchant.NormalizeTradeWant(
                    new TradeWant
                    {
                        Name = "firebow",
                        Level = 15,
                        Quantity = 4
                    })
                .Should()
                .Be(
                    new TradeWant
                    {
                        Name = "firebow",
                        Level = 12
                    });

        Merchant.NormalizeTradeWant(
                    new TradeWant
                    {
                        Name = "hpot0",
                        Level = 3,
                        Quantity = 50
                    })
                .Should()
                .Be(
                    new TradeWant
                    {
                        Name = "hpot0",
                        Quantity = 50
                    });

        Merchant.NormalizeTradeWant(
                    new TradeWant
                    {
                        Name = "placeholder"
                    })
                .Should()
                .BeNull();

        Merchant.NormalizeTradeWant(
                    new TradeWant
                    {
                        Name = "firebow",
                        Title = "no_such_title"
                    })
                .Should()
                .BeNull();
    }

    /// <summary>
    ///     The offer's match rule: the same name, at least its level and stack, and its title when it names one.
    /// </summary>
    [Test]
    public void ATradeWantAcceptsWhatTheServerAccepts()
    {
        var want = new TradeWant
        {
            Name = "firebow",
            Level = 5,
            Title = "shiny"
        };

        want.Accepts(
                "firebow",
                7,
                "shiny",
                1)
            .Should()
            .BeTrue();

        want.Accepts(
                "firebow",
                4,
                "shiny",
                1)
            .Should()
            .BeFalse("below the wanted level");

        want.Accepts(
                "firebow",
                7,
                null,
                1)
            .Should()
            .BeFalse("the offer names a title");

        want.Accepts(
                "firestaff",
                7,
                "shiny",
                1)
            .Should()
            .BeFalse();

        new TradeWant
            {
                Name = "hpot0",
                Quantity = 20
            }.Accepts(
                 "hpot0",
                 0,
                 null,
                 19)
             .Should()
             .BeFalse("short of the wanted stack");
    }

    /// <summary>
    ///     fury names four classes whose MainStat does not agree - several is real, not a formality.
    /// </summary>
    [Test]
    public void AnItemLockedToSeveralClassesNamesThemAll()
    {
        var fury = GameData.Items["fury"];

        fury!.Classes!.Count
             .Should()
             .Be(4);

        fury.Classes!.Should()
            .Contain(ALClass.Paladin);
    }

    /// <summary>
    ///     The game's own line about an item, read straight off the def.
    /// </summary>
    [Test]
    public void AnItemWithATooltipLineCarriesIt()
        => GameData.Items["firebow"]!.Explanation
                   .Should()
                   .Be("Rains fire upon the enemy");

    /// <summary>
    ///     The effect an item grants while worn, as the key the server files it under. Thirty items name one and the rest name
    ///     none.
    /// </summary>
    [Test]
    public void AnItemWithAnAbilityNamesIt()
    {
        GameData.Items["firebow"]!.Ability
                .Should()
                .Be("burn");

        GameData.Items["charmer"]!.Ability
                .Should()
                .Be("charm");

        GameData.Items["bow"]!.Ability
                .Should()
                .BeNull();
    }

    /// <summary>
    ///     Around half the items have nothing to say, and they say it with a null rather than an empty string.
    /// </summary>
    [Test]
    public void AnItemWithNoTooltipLineCarriesNull()
        => GameData.Items["hpot0"]!.Explanation
                   .Should()
                   .BeNull();
}