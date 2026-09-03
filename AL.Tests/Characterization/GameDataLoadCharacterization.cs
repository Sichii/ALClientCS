#region
using System.Reflection;
using System.Text.Json.Nodes;
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Classes;
using AL.Data.Images;
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

    /// <summary>
    ///     The server's own <c>free_cx</c> (js/old_common_functions.js:153), restated here rather than read back off
    ///     the production copy - a test that asked the push what the push should do would pass on any answer.
    /// </summary>
    private static readonly string[] FreeCosmetics =
    [
        "makeup105",
        "makeup117",
        "mmakeup00",
        "fmakeup01",
        "fmakeup02",
        "fmakeup03"
    ];

    /// <summary>
    ///     How many cosmetic names the class push owes each class in the snapshot, and how long its finished list is.
    ///     Only mage and priest are granted anything on the wire - three names each, none of which the push would have
    ///     added - so only their two numbers differ. Warrior carries an empty list; the other four carry no
    ///     <c>xcx</c> member at all, and none of the seven carries a null one.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, (int Pushed, int Total)> ExclusiveCosmeticCounts
        = new Dictionary<string, (int, int)>
        {
            ["warrior"] = (10, 10),
            ["paladin"] = (9, 9),
            ["rogue"] = (11, 11),
            ["ranger"] = (11, 11),
            ["mage"] = (8, 11),
            ["priest"] = (8, 11),
            ["merchant"] = (11, 11)
        };

    [Before(Class)]
    public static void PopulateFromSnapshot()
    {
        // T1 must assert the SNAPSHOT deterministically, but GameData is a shared static another test may already
        // have populated from the live API. Populate is not idempotent - FixLines rewrites the geometry line lists,
        // so a second call throws. Capture the statics, reset, load the snapshot; ClassCleanup restores them
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
        // one short of the snapshot's item count: emotionjar was dropped from the live data the members are
        // generated against, so the snapshot's copy of it binds to nothing
        GameData.Items
                .Keys
                .Count()
                .Should()
                .Be(578);

        // 67 rather than one key per served map: the datum declares the keys, so a map the snapshot carries but the
        // generated members do not know about binds to nothing, and a wire name differing from its CLR spelling by
        // more than case is cached under both.
        GameData.Maps
                .Keys
                .Count()
                .Should()
                .Be(67);

        GameData.Monsters
                .Keys
                .Count()
                .Should()
                .Be(136);

        GameData.Skills
                .Keys
                .Count()
                .Should()
                .Be(143);

        GameData.NPCs
                .Keys
                .Count()
                .Should()
                .Be(129);
    }

    /// <summary>
    ///     Pins the regeneration signal: the snapshot carries one member the generated datums do not declare, and an
    ///     injected unknown member is counted on top of it. The baseline alone could also mean the scan matched no
    ///     sections at all, which is why the injection half exists.
    /// </summary>
    [Test]
    public void T1_UnknownMemberScan_CountsOnlyUndeclaredMembers()
    {
        //a private parse - Fixture.GameData is shared, and this test injects a key
        var root = JsonNode.Parse(Fixture.GameDataJson)!.AsObject();

        var baseline = GameData.CountUnknownMembers(root);

        //emotionjar, the one item the snapshot still carries that the live data no longer does. A count above that
        //is the regeneration signal
        baseline.Should()
                .Be(1);

        ((JsonObject)root["items"]!)["an_item_no_datum_declares"] = new JsonObject();

        GameData.CountUnknownMembers(root)
                .Should()
                .Be(baseline + 1);
    }

    [Test]
    public void T1_EnrichedProperties_Populate()
    {
        // Exits and NPC Locations are [JsonIgnore] and filled by the enrichment pass, not the wire; they
        // prove Populate ran the map/NPC cross-linking, not just deserialization.
        GameData.Maps["main"]!.Exits
                .Count
                .Should()
                .Be(19);

        GameData.NPCs["citizen0"]!.Locations
                .Count
                .Should()
                .Be(1);
    }

    /// <summary>
    ///     Every name the push owes a class, worked out from the class's own looks rather than from the push: the free
    ///     makeups, then each look's name and each of its per-slot pieces, and never a name twice.
    /// </summary>
    private static IReadOnlyList<string> OwedCosmetics(string className)
    {
        var owed = new List<string>();

        foreach (var cosmetic in FreeCosmetics)
            if (!owed.Contains(cosmetic))
                owed.Add(cosmetic);

        foreach (var look in GameData.Classes[className]!.Looks)
        {
            if (!owed.Contains(look.Name))
                owed.Add(look.Name);

            foreach (var piece in look.Pieces.Values)
                if (!owed.Contains(piece))
                    owed.Add(piece);
        }

        return owed;
    }

    /// <summary>
    ///     The class half of the enrichment pass. The wire form carries the raw grant and nothing else, so a class is
    ///     only right once the load has restated the push the server does while it processes game data
    ///     (js/old_common_functions.js:171-182).
    /// </summary>
    [Test]
    public void T1_ClassCosmetics_CoverTheFreeMakeupsAndEveryPieceOfEveryLook()
    {
        foreach ((var className, (var pushed, var total)) in ExclusiveCosmeticCounts)
        {
            var owed = OwedCosmetics(className);

            owed.Should()
                .HaveCount(pushed, $"the snapshot owes {className} that many names");

            var exclusives = GameData.Classes[className]!.ExclusiveCosmetics;

            exclusives.Should()
                      .NotBeNull()
                      .And
                      .Contain(owed, $"{className} may wear every one of them without owning it")
                      .And
                      .OnlyHaveUniqueItems($"each push is guarded, so {className} cannot list a name twice")
                      .And
                      .HaveCount(total, $"{className} is owed {pushed} and granted the rest on the wire");
        }
    }

    /// <summary>
    ///     The hazard the push covers, stated from the raw entry as well as from the loaded one: four of the seven
    ///     classes carry no exclusive list at all, so binding alone would leave them nothing to wear.
    /// </summary>
    [Test]
    public void T1_ClassCosmetics_AClassTheSnapshotCarriesNoListForStillEndsUpWithOne()
    {
        var raw = Fixture.Entry("classes", "paladin")
                         .AsObject();

        raw.Should()
           .NotContainKey("xcx", "the snapshot carries no exclusive list for this class");

        TestJson.Data<GClass>(raw.ToJsonString())!
                .ExclusiveCosmetics
                .Should()
                .NotBeNull()
                .And
                .BeEmpty();

        GameData.Classes["paladin"]!
                .ExclusiveCosmetics
                .Should()
                .NotBeNull()
                .And
                .NotBeEmpty();
    }
    /// <summary>
    ///     A look the wire sends short, or with nothing in its second slot, binds with a null piece map.
    /// </summary>
    /// <remarks>
    ///     This is the premise the null guards in <c>EnrichClasses</c> rest on, and it is surprising enough to
    ///     pin on its own: the positional converter leaves a reference-typed argument null rather than defaulting
    ///     it, so the map is null rather than empty, and walking it throws inside <c>Populate</c> - a client that
    ///     will not start, not a look that will not draw. The server's own loop over that slot tolerates the
    ///     same shape and keeps going.
    ///     <br />
    ///     Every look in the snapshot is well formed, so nothing else here would notice if this changed. If the
    ///     converter ever starts defaulting the map, this test says so and those guards can go.
    /// </remarks>
    [Test]
    [Arguments("[\"marmor4b\"]", "the array stops before the piece map")]
    [Arguments("[\"marmor4b\", null]", "the piece map is sent as null")]
    public void ALookMissingItsPieceMapBindsNull(string wire, string because)
        => TestJson.Data<GClassLook>(wire)!
                   .Pieces
                   .Should()
                   .BeNull(because);

    /// <summary>
    ///     ExchangeAtNPC is enriched for every item carrying an exchange count, which is what makes the exchange
    ///     errand able to walk anywhere for one. The counts are asserted rather than a null check because the
    ///     direction that matters is the reverse one: the field was previously filled in from an NPC's token, which
    ///     reached four items, and narrowing it back that far would look harmless and leave 34 exchangeables with
    ///     nowhere to go. These are the snapshot's counts, and the errands' prose cites the snapshot too, attributing
    ///     it, so this assertion is what holds those comments as well as the rule. That is the whole reason to cite the
    ///     fixture: it is the only number anything can check.
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
                     .Be(40);

        exchangeables.Should()
                     .OnlyContain(item => item.ExchangeAtNPC != null);

        // the server measures a quest-tagged exchangeable against that quest's NPC and everything else against the
        // one fixed placement (node/server.js:6073), so the split is the rule itself rather than a tally
        exchangeables.Count(item => item.ExchangeAtNPC!.Id == "exchange")
                     .Should()
                     .Be(32);

        exchangeables.Count(item => item.ExchangeAtNPC!.Id != "exchange")
                     .Should()
                     .Be(8);

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
                .Be(52);

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
        // Version is the strong static-binding canary: 5200 in the committed snapshot, 0 if static binding breaks.
        GameData.Version
                .Should()
                .Be(5200);

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
                .HaveCount(102);

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

    /// <summary>
    ///     The three halves of the drop data that used to be discarded. The prize tables are keyed by the game's own
    ///     drop id, which is an item name for most of them and something else entirely for the rest.
    /// </summary>
    [Test]
    public void T1_Drops_BindTheRestOfTheTable()
    {
        GameData.Drops
                .Maps
                .Should()
                .HaveCount(11);

        //the two the game ships empty, which a shape that only bound non-empty tables would silently lose
        GameData.Drops
                .Maps["global"]
                .Should()
                .BeEmpty();

        GameData.Drops
                .Maps["main"]
                .Should()
                .Contain(drop => drop.Name == "ringsj");

        GameData.Drops
                .Konami
                .Should()
                .Contain(drop => drop.Name == "goldenpowerglove");

        //65 of the 67 leftover keys - skins and monsters_home_server are objects rather than drop lists, and the
        //shape guard drops both
        GameData.Drops
                .Tables
                .Should()
                .HaveCount(65);

        //the typed keys are typed, so none of them reaches the leftovers
        GameData.Drops
                .Tables
                .Keys
                .Should()
                .NotContain(["gold", "monsters", "maps", "konami"]);

        //a drop id that is not an item at all, which is why the raw table has to stay reachable
        GameData.Drops
                .Tables
                .Should()
                .ContainKey("xN");

        GameData.Drops
                .Tables["GEM0"]
                .Should()
                .Contain(drop => drop.Name == "weaponbox");
    }

    /// <summary>
    ///     The exchange prizes, keyed the way the server keys them. The lost earring is the whole reason the key is a
    ///     level rather than a name: its five tables are five different prizes, not five rates on one.
    /// </summary>
    [Test]
    public void T1_ExchangeRewards_AreKeyedByTheLevelExchanged()
    {
        //an ordinary exchangeable takes no levels, so the server rolls its bare name and there is one table
        GameData.Items["gem0"]!
                .ExchangeRewards
                .Should()
                .ContainKey(0)
                .And
                .HaveCount(1);

        GameData.Items["lostearring"]!
                .ExchangeRewards
                .Should()
                .ContainKeys(0, 1, 2, 3, 4);

        GameData.Items["lostearring"]!
                .ExchangeRewards![2]
                .Select(drop => drop.Name)
                .Should()
                .BeEquivalentTo(["wbook1", "t2quiver"]);

        //the +0 table opens another table rather than handing an item over
        GameData.Items["lostearring"]!
                .ExchangeRewards![0]
                .Single()
                .IsChest
                .Should()
                .BeTrue();

        //nothing exchangeable about it, so nothing to roll
        GameData.Items["hpot0"]!
                .ExchangeRewards
                .Should()
                .BeNull();
    }

    [Test]
    public void T1_MapDrops_AreTheMapsOwnTable()
    {
        GameData.Maps["main"]!
                .Drops
                .Should()
                .Contain(drop => drop.Name == "ringsj");

        //empty rather than null, so a caller never has to ask which kind of nothing it got
        GameData.Maps["desertland"]!
                .Drops
                .Should()
                .BeEmpty();
    }

    /// <summary>
    ///     The wardrobe tables, which are the two ways an owned name is not the name that gets worn.
    /// </summary>
    [Test]
    public void T1_Cosmetics_Bind()
    {
        //one name standing for two sprites, and neither of the two is the name the account owns
        GameData.Cosmetics
                .Bundle["rogueb"]
                .Should()
                .BeEquivalentTo(["sbody1c", "sarmor1h"]);

        GameData.Cosmetics
                .Map["old"]
                .Should()
                .Be("new");

        //ordinal, matching the server's own object lookup. A case-insensitive table here would answer that a
        //differently-cased name is wearable, and the emit built on that answer comes back cx_not_found
        GameData.Cosmetics
                .Bundle
                .ContainsKey("ROGUEB")
                .Should()
                .BeFalse();

        GameData.Cosmetics
                .Prop["marmor12c"]
                .Should()
                .BeEquivalentTo(["covers", "no_hair"]);

        //empty rather than null, so a caller never has to ask which kind of nothing it got
        GameData.Cosmetics
                .NoUpper
                .Should()
                .BeEmpty();
    }

    /// <summary>
    ///     The name-to-type table the server builds by walking every typed sheet's matrix
    ///     (js/old_common_functions.js:183-193), restated here rather than read back out of production code: a probe
    ///     sharing the rule under test agrees with itself whatever the rule does.
    /// </summary>
    /// <remarks>
    ///     This is what turns a cosmetic name into a slot, so a <see cref="GSprite.Type" /> that bound null for every
    ///     sheet would compile, pass a null check, and still leave nothing placeable. The counts are what catches
    ///     that: the rebuilt table is empty without it, and with it resolves every name the four catalogues carry bar
    ///     one. Untyped sheets are left out rather than defaulted to the server's <c>full</c> placeholder, which
    ///     reaches no slot.
    /// </remarks>
    [Test]
    public void T1_SpriteTypes_ResolveTheCosmeticCatalogues()
    {
        GameData.Sprites
                .Values
                .Count(sprite => sprite.Type is not null)
                .Should()
                .Be(77);

        //the absence is null, never an empty string - the two would otherwise both read as "no type" while only
        //one of them can be told apart from a sheet the data has yet to classify
        GameData.Sprites
                .Values
                .Should()
                .NotContain(sprite => sprite.Type == string.Empty);

        var typeOfName = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var sprite in GameData.Sprites.Values)
        {
            if (sprite.Type is null)
                continue;

            foreach (var name in sprite.Matrix.SelectMany(row => row))
                if (name is not null)
                    typeOfName[name] = sprite.Type;
        }

        typeOfName.Should()
                  .HaveCount(712);

        //the type every catalogued name carries is the catalogue's own name, so counting the names that agree is a
        //cross-check between two tables rather than two readings of one
        (string Slot, IEnumerable<string> Names)[] catalogues =
        [
            ("head", GameData.Cosmetics.Head.Keys),
            ("hair", GameData.Cosmetics.Hair.Keys),
            ("hat", GameData.Cosmetics.Hat.Keys),
            ("gravestone", GameData.Cosmetics.Gravestone.Keys)
        ];

        var agreeing = catalogues.ToDictionary(
            catalogue => catalogue.Slot,
            catalogue => catalogue.Names.Count(name => typeOfName.GetValueOrDefault(name) == catalogue.Slot));

        agreeing["head"]
            .Should()
            .Be(66);

        agreeing["hair"]
            .Should()
            .Be(124);

        agreeing["hat"]
            .Should()
            .Be(1);

        agreeing["gravestone"]
            .Should()
            .Be(7);

        //the 199th name, catalogued but on no sheet. That is the data rather than a binding fault, and pinning it by
        //name is what keeps a second unresolvable name from hiding inside a count
        catalogues.SelectMany(catalogue => catalogue.Names)
                  .Where(name => !typeOfName.ContainsKey(name))
                  .Should()
                  .BeEquivalentTo(["hairdo523"]);
    }
}