#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AL.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T1 - pins that <see cref="GameData.Populate" /> binds every static datum from the committed
///     snapshot, under Newtonsoft, so the System.Text.Json migration has a fixed target. Asserts real
///     values (counts, scalars) rather than <c>IsNotNull</c>: a null check passes whether a datum bound
///     520 items or zero.
/// </summary>
/// <remarks>
///     <see cref="GameData" /> is static global state. This suite populates it from the offline snapshot
///     rather than the live API (<see cref="GameDataTestBed" />) so the pinned values never move with the
///     game. The shared statics are captured before and restored after so the rest of the suite keeps its
///     own (live) data - see <see cref="PopulateFromSnapshot" /> / <see cref="RestoreGameData" />.
/// </remarks>
[TestClass]
public class GameDataLoadCharacterization
{
    private static readonly FieldInfo[] StaticBackingFields = typeof(GameData)
                                                              .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                                                              .Where(field => !field.IsLiteral && !field.IsInitOnly)
                                                              .ToArray();

    private static Dictionary<FieldInfo, object?> CapturedState = new();

    [ClassInitialize]
    public static void PopulateFromSnapshot(TestContext _)
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

    [ClassCleanup]
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

    [TestMethod]
    public void T1_StaticScalars_Bind()
    {
        // Version is the strong static-binding canary: 804 under Newtonsoft, 0 if static binding breaks.
        Assert.AreEqual(804, GameData.Version);

        // Multipliers replaces the phantom top-level "inflation"/"shells_to_gold" keys; buy_to_sell is the
        // NPC buy-back ratio every sell price derives from, so a 0 here means the nested bind broke.
        Assert.AreEqual(32000, GameData.Multipliers.ShellsToGold);
        Assert.AreEqual(32000, GameData.ShellsToGold);
        Assert.AreEqual(0.6f, GameData.Multipliers.BuyToSell);
        Assert.AreEqual(2f, GameData.Multipliers.SecondHandsMult);

        // Levels binds as a dictionary; 200 vs its empty initializer proves the static dictionary bound.
        Assert.AreEqual(200, GameData.Levels.Count);
    }

    [TestMethod]
    public void T1_Datums_FullyPopulated()
    {
        Assert.AreEqual(520, GameData.Items.Keys.Count());

        // 61 not 69: live G renamed d_a1/d_a2/d_b1/d_g to ucliffs/uhills/spider_instance/gateway, so the datum
        // no longer declares those keys and the Nov-2024 fixture's four now-unbound entries drop 8 cache keys.
        Assert.AreEqual(61, GameData.Maps.Keys.Count());
        Assert.AreEqual(128, GameData.Monsters.Keys.Count());
        Assert.AreEqual(127, GameData.Skills.Keys.Count());
        Assert.AreEqual(127, GameData.NPCs.Keys.Count());
    }

    [TestMethod]
    public void T1_KnownEntries_HaveExpectedValues()
    {
        // GoldValue binds to wire key "g", not "gold" (the gold-find stat, ~0 for most items).
        Assert.AreEqual(96000f, GameData.Items["fireblade"]!.GoldValue);

        // Attack arrives through the attributed-object harvest, not a declared wire property on GMonster.
        Assert.AreEqual(5f, GameData.Monsters["goo"]!.Attack);
    }

    [TestMethod]
    public void T1_Geometry_JsonIncludeCanary()
    {
        // HorizontalLines/VerticalLines have an internal setter bound via ItemConverterType. If that member
        // fails to bind post-migration it keeps its new List<>() initializer, and AddBorderWalls leaves only
        // the 2 border walls per axis - the pathfinder then treats "main" as fully walkable. 760 is the canary.
        var geometry = GameData.Geometry["main"]!;
        Assert.AreEqual(760, geometry.HorizontalLines.Count);
        Assert.AreEqual(760, geometry.VerticalLines.Count);
    }

    [TestMethod]
    public void T1_NonPublicSetter_Binds()
    {
        // CooldownMS has a private setter; a missed [JsonInclude] post-migration is a silent zero. "attack"
        // has no wire cooldown (genuinely 0), so cleave/supershot are the meaningful non-public-setter canary.
        Assert.AreEqual(0, GameData.Skills["attack"]!.CooldownMS);
        Assert.AreEqual(1200, GameData.Skills["cleave"]!.CooldownMS);
        Assert.AreEqual(30000, GameData.Skills["supershot"]!.CooldownMS);
    }

    [TestMethod]
    public void T1_EnrichedProperties_Populate()
    {
        // Exits and NPC Locations are [JsonIgnore] and filled by the enrichment pass, not the wire; they
        // prove Populate ran the map/NPC cross-linking, not just deserialization.
        Assert.AreEqual(18, GameData.Maps["main"]!.Exits.Count);
        Assert.AreEqual(1, GameData.NPCs["citizen0"]!.Locations.Count);
    }
}
