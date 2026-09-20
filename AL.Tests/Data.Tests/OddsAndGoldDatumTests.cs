#region
using AL.Data.Compounds;
using AL.Data.MonsterGold;
using AL.Data.Upgrades;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The three tables the payload began carrying at game data version 16846: the two bench odds tables and the gold a
///     kill pays.
/// </summary>
public class OddsAndGoldDatumTests
{
    /// <summary>
    ///     A row is keyed by the item's grade and, inside it, by the level being reached - the server's own indexing. A grade
    ///     or a level the payload leaves out reads as no entry rather than a chance of nothing.
    /// </summary>
    [Test]
    public void CompoundRowsBindByGradeThenByLevelReached()
    {
        var compounds = TestJson.Data<CompoundsDatum>("""{"0":{"1":0.99,"2":0.75},"2":{"1":0.8}}""")!;
        compounds.BuildLookupTable();

        compounds.Grade0[2]
                 .Should()
                 .Be(0.75);

        compounds["2"]![1]
            .Should()
            .Be(0.8);

        compounds["1"]
            .Should()
            .BeNull();

        compounds.ChanceOf(2, 1)
                 .Should()
                 .Be(0.8);

        compounds.ChanceOf(1, 1)
                 .Should()
                 .BeNull("the payload carried no grade 1 row");

        compounds.ChanceOf(0, 3)
                 .Should()
                 .BeNull("the grade 0 row stops at +2");
    }

    /// <summary>
    ///     Gold binds by monster, and a monster the payload does not price is absent from the lookup rather than priced at
    ///     nothing - the table leaves out the instance-only monsters, and a zero would read as a real figure.
    /// </summary>
    [Test]
    public void MonsterGoldBindsByMonsterAndLeavesUnpricedOnesOut()
    {
        var gold = TestJson.Data<MonsterGoldDatum>("""{"goo":20,"bee":40}""")!;
        gold.BuildLookupTable();

        gold.Goo
            .Should()
            .Be(20);

        gold["bee"]
            .Should()
            .Be(40);

        gold["crab"]
            .Should()
            .BeNull();

        gold.Entries
            .Should()
            .HaveCount(2);
    }

    /// <summary>
    ///     The upgrade track runs to +12 where the compound one stops at +10; the shape is otherwise the same.
    /// </summary>
    [Test]
    public void UpgradeRowsBindByGradeThenByLevelReached()
    {
        var upgrades = TestJson.Data<UpgradesDatum>("""{"1":{"12":0.1}}""")!;
        upgrades.BuildLookupTable();

        upgrades.Grade1[12]
                .Should()
                .Be(0.1);

        upgrades.ChanceOf(1, 12)
                .Should()
                .Be(0.1);

        upgrades["0"]
            .Should()
            .BeNull();
    }
}