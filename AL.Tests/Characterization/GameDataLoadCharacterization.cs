#region
using System.Reflection;
using AL.Core.Definitions;
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T1 - pins that <see cref="GameData.Populate" /> binds every static datum from the committed snapshot. The values
///     were frozen pre-migration, so the System.Text.Json port had a fixed target. Asserts real values (counts, scalars)
///     rather than
///     <c>
///         IsNotNull
///     </c>
///     : a null check passes whether a datum bound 520 items or zero.
/// </summary>
/// <remarks>
///     <see cref="GameData" /> is static global state. This suite populates it from the offline snapshot rather than the
///     live API (<see cref="GameDataTestBed" />) so the pinned values never move with the game. The shared statics are
///     captured before and restored after so the rest of the suite keeps its own (live) data - see
///     <see cref="PopulateFromSnapshot" /> / <see cref="RestoreGameData" />.
/// </remarks>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class GameDataLoadCharacterization
{
    private static readonly FieldInfo[] StaticBackingFields = typeof(GameData).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                                                                              .Where(field => !field.IsLiteral && !field.IsInitOnly)
                                                                              .ToArray();

    private static Dictionary<FieldInfo, object?> CapturedState = new();

    [Before(Class)]
    public static void PopulateFromSnapshot()
    {
        // T1 must assert the SNAPSHOT deterministically, but GameData is a shared static another test may
        // already have populated from the live API (a different Version, map count, etc.). Populate is not
        // idempotent - FixLines rewrites the geometry line lists to arrays, so a second call throws in
        // AddBorderWalls. Capture the current statics, reset them so the re-Populate binds fresh instances,
        // then load the snapshot. ClassCleanup restores the captured state so the rest of the run is
        // unaffected regardless of test execution order.
        CapturedState = StaticBackingFields.ToDictionary(field => field, field => field.GetValue(null));

        SetStaticsToDefault();
        GameData.Populate(Fixture.GameDataJson);
    }

    [After(Class)]
    public static void RestoreGameData()
    {
        foreach ((var field, var value) in CapturedState)
            field.SetValue(null, value);
    }

    private static void SetStaticsToDefault()
    {
        foreach (var field in StaticBackingFields)
        {
            var defaultValue = field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null;
            field.SetValue(null, defaultValue);
        }
    }

    [Test]
    public void T1_Datums_FullyPopulated()
    {
        GameData.Items
                .Keys
                .Count()
                .Should()
                .Be(520);

        // 61 not 69: live G renamed d_a1/d_a2/d_b1/d_g to ucliffs/uhills/spider_instance/gateway, so the datum
        // no longer declares those keys and the Nov-2024 fixture's four now-unbound entries drop 8 cache keys.
        GameData.Maps
                .Keys
                .Count()
                .Should()
                .Be(61);

        GameData.Monsters
                .Keys
                .Count()
                .Should()
                .Be(128);

        GameData.Skills
                .Keys
                .Count()
                .Should()
                .Be(127);

        GameData.NPCs
                .Keys
                .Count()
                .Should()
                .Be(127);
    }

    [Test]
    public void T1_EnrichedProperties_Populate()
    {
        // Exits and NPC Locations are [JsonIgnore] and filled by the enrichment pass, not the wire; they
        // prove Populate ran the map/NPC cross-linking, not just deserialization.
        GameData.Maps["main"]!.Exits
                .Count
                .Should()
                .Be(18);

        GameData.NPCs["citizen0"]!.Locations
                .Count
                .Should()
                .Be(1);
    }

    /// <summary>
    ///     ExchangeAtNPC is enriched for every item carrying an exchange count, which is what makes the exchange
    ///     errand able to walk anywhere for one. The counts are asserted rather than a null check because the
    ///     direction that matters is the reverse one: the field was previously filled in from an NPC's token, which
    ///     reached four items, and narrowing it back that far would look harmless and leave 34 exchangeables with
    ///     nowhere to go. These are the snapshot's counts, not the served table's, which carries 39 - and the errands'
    ///     prose cites the snapshot too, attributing it, so this assertion is what holds those comments as well as the
    ///     rule. That is the whole reason to cite the fixture: it is the only number anything can check.
    /// </summary>
    [Test]
    public void T1_ExchangeAtNPC_CoversEveryExchangeable()
    {
        var exchangeables = GameData.Items
                                    .Values
                                    .DistinctBy(item => item.Accessor)
                                    .Where(item => item.ExchangeCount.HasValue)
                                    .ToList();

        exchangeables.Count
                     .Should()
                     .Be(38);

        exchangeables.Should()
                     .OnlyContain(item => item.ExchangeAtNPC != null);

        // the server measures a quest-tagged exchangeable against that quest's NPC and everything else against the
        // one fixed placement (node/server.js:6073), so the split is the rule itself rather than a tally
        exchangeables.Count(item => item.ExchangeAtNPC!.Id == "exchange")
                     .Should()
                     .Be(31);

        exchangeables.Count(item => item.ExchangeAtNPC!.Id != "exchange")
                     .Should()
                     .Be(7);

        GameData.Items["leather"]!
                .ExchangeAtNPC!.Id
                .Should()
                .Be("leathermerchant");

        GameData.Items["gem0"]!
                .ExchangeAtNPC!.Id
                .Should()
                .Be("exchange");
    }

    /// <summary>
    ///     Every buyable item resolves to a seller that is actually standing somewhere. <c>EnrichItems</c> races
    ///     first-writer-wins over <c>NPCs.Values</c>, which is declaration order rather than wire order, and
    ///     <c>CanBuy</c> ends on <c>ObtainableFromNPC.Locations.Any(…)</c> - so an item resolved to a seller placed
    ///     only on <c>ignore: true</c> maps is unbuyable with nothing logged at all, which is what the placed-first
    ///     ordering there exists to prevent.
    ///     <br />
    ///     <b>What this does and does not pin.</b> It cannot distinguish the ordering being present from absent,
    ///     because on this snapshot every item's first seller happens to be placed already - the fix is a no-op on
    ///     today's data and only removes the hazard. What it does catch is the data moving underneath: `pots` and
    ///     `weapons` both carry item lists and stand only on <c>old_main</c>/<c>original_main</c>, so the day a seller
    ///     ahead of them loses its placement, or one of their 8 items loses its other seller, this goes red instead of
    ///     the merchant silently buying nothing. Inverting the ordering to prefer unplaced sellers fails it, which is
    ///     how it was checked.
    /// </summary>
    [Test]
    public void T1_ObtainableFromNPC_ResolvesToAPlacedSeller()
    {
        var buyables = GameData.Items
                               .Values
                               .DistinctBy(item => item.Accessor)
                               .Where(item => item.ObtainType == ObtainType.Buy)
                               .ToList();

        buyables.Count
                .Should()
                .Be(50);

        buyables.Should()
                .OnlyContain(item => item.ObtainableFromNPC!.Locations.Count > 0);
    }

    [Test]
    public void T1_Geometry_JsonIncludeCanary()
    {
        // HorizontalLines/VerticalLines have an internal setter bound via ItemConverterType. If that member
        // fails to bind post-migration it keeps its new List<>() initializer, and AddBorderWalls leaves only
        // the 2 border walls per axis - the pathfinder then treats "main" as fully walkable. 760 is the canary.
        var geometry = GameData.Geometry["main"]!;

        geometry.HorizontalLines
                .Count
                .Should()
                .Be(760);

        geometry.VerticalLines
                .Count
                .Should()
                .Be(760);
    }

    [Test]
    public void T1_KnownEntries_HaveExpectedValues()
    {
        // GoldValue binds to wire key "g", not "gold" (the gold-find stat, ~0 for most items).
        GameData.Items["fireblade"]!.GoldValue
                .Should()
                .Be(96000f);

        // Attack arrives through the attributed-object harvest, not a declared wire property on GMonster.
        GameData.Monsters["goo"]!.Attack
                .Should()
                .Be(5f);
    }

    [Test]
    public void T1_NonPublicSetter_Binds()
    {
        // CooldownMS has a private setter; a missed [JsonInclude] post-migration is a silent zero. "attack"
        // has no wire cooldown (genuinely 0), so cleave/supershot are the meaningful non-public-setter canary.
        GameData.Skills["attack"]!.CooldownMS
                .Should()
                .Be(0);

        GameData.Skills["cleave"]!.CooldownMS
                .Should()
                .Be(1200);

        GameData.Skills["supershot"]!.CooldownMS
                .Should()
                .Be(30000);
    }

    [Test]
    public void T1_StaticScalars_Bind()
    {
        // Version is the strong static-binding canary: 804 pre-migration, 0 if static binding breaks.
        GameData.Version
                .Should()
                .Be(804);

        // Multipliers replaces the phantom top-level "inflation"/"shells_to_gold" keys; buy_to_sell is the
        // NPC buy-back ratio every sell price derives from, so a 0 here means the nested bind broke.
        GameData.Multipliers
                .ShellsToGold
                .Should()
                .Be(32000);

        GameData.ShellsToGold
                .Should()
                .Be(32000);

        GameData.Multipliers
                .BuyToSell
                .Should()
                .Be(0.6f);

        GameData.Multipliers
                .SecondHandsMult
                .Should()
                .Be(2f);

        // Levels binds as a dictionary; 200 vs its empty initializer proves the static dictionary bound.
        GameData.Levels
                .Count
                .Should()
                .Be(200);
    }

    /// <summary>
    ///     The drop entries are the one positional shape whose third slot means two different things, so all three
    ///     forms are pinned here rather than only the count.
    /// </summary>
    [Test]
    public void T1_Drops_Bind()
    {
        GameData.Drops
                .Gold
                .Base
                .Should()
                .Be(0.64f);

        GameData.Drops
                .Gold
                .X50
                .Should()
                .BeApproximately(1f / 480f, 0.000001f);

        GameData.Drops
                .Monsters
                .Should()
                .HaveCount(87);

        // [rate, item] - the plain form, 220 of the 323 entries
        var seashell = GameData.Drops.Monsters["crab"]
                               .Single(drop => drop.Name == "seashell");

        seashell.Rate
                .Should()
                .Be(0.005f);

        seashell.Quantity
                .Should()
                .Be(1);

        seashell.IsChest
                .Should()
                .BeFalse();

        // [rate, item, quantity] - the third slot as a count
        GameData.Drops.Monsters["goo"]
                .Single(drop => drop.Name == "shells")
                .Quantity
                .Should()
                .Be(50);

        // [rate, "open", table] - the third slot as the name of a further table, which the second slot would
        // otherwise hold. Binding this positionally would read the marker as the item and the table as a count
        var chest = GameData.Drops.Monsters["rgoo"]
                            .Single(drop => drop.IsChest);

        chest.Name
             .Should()
             .Be("lglitch");

        chest.Rate
             .Should()
             .Be(0.025f);
    }
}