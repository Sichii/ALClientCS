#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AL.APIClient.Request;
using AL.Client;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Data;
using AL.Pathfinding;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T11 — Non-public setter census. Pins, under Newtonsoft, every member the migration must decorate
///     with <c>[JsonInclude]</c> or STJ will silently bind it to its default.
/// </summary>
/// <remarks>
///     The census is built by reflection so it cannot drift out of sync with the code: any newly added
///     non-public JSON member changes the committed fixture and fails <see cref="T11_Census_MatchesCommittedFixture" />.
///     The value assertions then prove that a real <c>start</c>/<c>player</c>/<c>entities</c> frame actually
///     drives those members away from their defaults under the current serializer.
/// </remarks>
[TestClass]
public sealed class NonPublicMemberCharacterization
{
    private const string CENSUS_SNAPSHOT = "t11-nonpublic-member-census.txt";

    private const string GENERATED_CENSUS_SNAPSHOT = "t11-nonpublic-member-census.generated.txt";

    private const string START_FRAME = "t11-start-frame.json";

    private const string PLAYER_FRAME = "t11-player-frame.json";

    private const string ENTITIES_FRAME = "t11-entities-frame.json";

    // Category tags used in the committed census.
    private const string FIELD = "field";

    private const string NON_PUBLIC_PROPERTY = "non-public-property";

    private const string NON_PUBLIC_SETTER = "non-public-setter";

    /// <summary>
    ///     The six library assemblies whose types the migration decorates. Resolved through one representative
    ///     public type per assembly so a rename fails to compile rather than silently narrowing the census.
    /// </summary>
    private static readonly IReadOnlyList<Assembly> LibraryAssemblies = new[]
    {
        typeof(AttributedObjectBase).Assembly, // AL.Core
        typeof(GameData).Assembly,             // AL.Data
        typeof(ALClient).Assembly,             // AL.Client
        typeof(StartData).Assembly,            // AL.SocketClient
        typeof(LoginInfo).Assembly,            // AL.APIClient
        typeof(Pathfinder).Assembly            // AL.Pathfinding
    }.Distinct()
     .ToArray();

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void T11_Census_MatchesCommittedFixture()
    {
        var census = BuildCensus();
        var rendered = Render(census);

        //written under a distinct name, so regenerating never overwrites the committed fixture's output path and
        //turns the comparison into a tautology - the hazard MapBoundaryCharacterization's sidecar already avoids
        var generatedPath = Fixture.WriteSnapshot(GENERATED_CENSUS_SNAPSHOT, rendered);
        var committed = Fixture.ReadCommittedSnapshot(CENSUS_SNAPSHOT);

        committed.Should()
                 .NotBeNull(
                     $"the census fixture must be committed at AL.Tests/Fixtures/snapshots/{CENSUS_SNAPSHOT}; "
                     + $"copy the generated file from {generatedPath} into the repo and re-run");

        Normalize(committed!)
            .Should()
            .Be(
                Normalize(rendered),
                "the non-public JSON member census drifted; re-commit the generated snapshot if the change is intended");
    }

    [TestMethod]
    public void T11_Census_PinsCategoryCounts()
    {
        var census = BuildCensus();

        var instanceNonPublicSetters = census.Count(entry => entry is { Category: NON_PUBLIC_SETTER, IsStatic: false });
        var staticNonPublicSetters = census.Count(entry => entry is { Category: NON_PUBLIC_SETTER, IsStatic: true });
        var nonPublicProperties = census.Count(entry => entry.Category == NON_PUBLIC_PROPERTY);
        var fields = census.Count(entry => entry.Category == FIELD);

        // Diagnostics for the two migration hazards the census surfaces.
        var deadEnumFieldProperties = DeadStaticFieldPropertyCount();
        var allInstanceNonPublicSetters = AllInstanceNonPublicSetterCount();
        var auditGap = allInstanceNonPublicSetters - instanceNonPublicSetters;

        TestContext.WriteLine($"instance non-public setters (w/ [JsonProperty]) : {instanceNonPublicSetters}");
        TestContext.WriteLine($"static  non-public setters (GameData)          : {staticNonPublicSetters}");
        TestContext.WriteLine($"non-public properties                          : {nonPublicProperties}");
        TestContext.WriteLine($"instance fields                                : {fields}");
        TestContext.WriteLine($"total census entries                           : {census.Count}");
        TestContext.WriteLine($"dead static-field [JsonProperty] (enum members): {deadEnumFieldProperties}");
        TestContext.WriteLine($"ALL instance non-public setters (any/no attr)  : {allInstanceNonPublicSetters}");
        TestContext.WriteLine($"[JsonProperty]-gated audit BLIND SPOT          : {auditGap}");

        // Pinned to the REAL reflection result. Phase 1 moved IAttributed's [JsonProperty] attributes down onto
        // AttributedObjectBase / AttributedRecordBase, so the ~54 attributed stat setters now carry a class-level
        // [JsonProperty] and join the census: instance non-public setters rose from 54 to 108. The 8 non-public
        // properties and 5 instance fields are unchanged.
        // Phase 6 (GD-08) deleted the phantom GameData.Inflation static, dropping static GameData setters 21 -> 20.
        // Phase 7 (entity key-presence merge + J4): EntityBase gained MaxMP + Focus (+2, both [JsonProperty]);
        // Player shed its now-shadowing Focus/MaxMP duplicates (-2); Character shed the three start-only fields
        // Emotion/Friends/OwnedCosmetics (-3), moved onto ALClient so ShallowMerge stops wiping them. Net -3 -> 105.
        // Phase 11 added eight Character owner-stats (max_xp/goldm/xpm/luckm/cash/incdmgamp/mcourage/pcourage),
        // each a [JsonProperty] protected-set non-public setter on an entity frame type: +8 -> 113.
        instanceNonPublicSetters.Should().Be(113);
        staticNonPublicSetters.Should().Be(20);
        nonPublicProperties.Should().Be(8);
        fields.Should().Be(5);

        // Finding: ~48 [JsonProperty] on BankPack (and a few other) enum members are dead — Newtonsoft ignores
        // JsonProperty on enum members and STJ never binds a literal. They are NOT [JsonInclude] targets.
        deadEnumFieldProperties.Should().Be(48);

        // Finding, and the reason a [JsonProperty]-gated audit is still unsafe: 139 instance properties in the six
        // assemblies have a non-public setter Newtonsoft binds (140 before MpCost was deleted), and 108 now carry
        // a class-level [JsonProperty] after the Phase 1 move, so an audit keyed on [JsonProperty] is still blind
        // to 31 of them — e.g. EntityBase.In and the other setters the server sends but no member decorates. The
        // Phase 3 audit must key on "non-public accessor", never on "has [JsonProperty]".
        // Phase 7 raised the gap 31 -> 35: EntityBase.PresentFields (a [JsonIgnore] private-set field of the
        // key-presence merge) and the three Emotion/Friends/OwnedCosmetics setters now on ALClient (populated
        // imperatively from StartData, so intentionally un-attributed) are non-public setters with no
        // [JsonProperty] - not missed bindings, but they widen the [JsonProperty]-gated blind spot all the same.
        // Phase 8 raised it 35 -> 37: ALSocketClient.LastDisconnectReason and ALClient.FatalError are public-get
        // private-set status fields (disconnect reason / terminal-failure flag), set imperatively and never JSON
        // bound - two more un-attributed non-public setters, not missed bindings. LimitDcReportData's members are
        // init-only (a public setter) so they are not counted, and the two new enum members carry no [JsonProperty].
        auditGap.Should().Be(37);
        // Phase 11: +8 Character owner-stats, all [JsonProperty], so they widen no blind spot -> 150.
        allInstanceNonPublicSetters.Should().Be(150);
    }

    /// <summary>
    ///     Every instance property in the six assemblies whose property is public but whose set accessor is
    ///     non-public — the true [JsonInclude] target set, independent of whether the member carries a
    ///     Newtonsoft [JsonProperty].
    /// </summary>
    private static int AllInstanceNonPublicSetterCount()
    {
        var count = 0;

        foreach (var assembly in LibraryAssemblies)
        foreach (var type in GetTypes(assembly))
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var getter = property.GetGetMethod(nonPublic: true);
            var setter = property.GetSetMethod(nonPublic: true);
            var propertyIsPublic = (getter?.IsPublic ?? false) || (setter?.IsPublic ?? false);

            if (propertyIsPublic && setter is { IsPublic: false })
                count++;
        }

        return count;
    }

    /// <summary>
    ///     Static/literal fields carrying [JsonProperty] — dead decoration (enum members). Never a bind target.
    /// </summary>
    private static int DeadStaticFieldPropertyCount()
    {
        var count = 0;

        foreach (var assembly in LibraryAssemblies)
        foreach (var type in GetTypes(assembly))
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (field.IsStatic && field.GetCustomAttribute<JsonPropertyAttribute>(inherit: false) is not null)
                count++;
        }

        return count;
    }

    [TestMethod]
    public void T11_Census_ContainsEveryNamedSite()
    {
        var census = BuildCensus();

        // The eight non-public *properties* the plan names (property visibility itself is non-public).
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Skills.GSkill", "ListTargets");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Skills.GSkill", "ReuseCooldown");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Maps.GMapMonster", "_boundaries");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Maps.GMapMonster", "_boundary");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Maps.GMapNPC", "_position");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.Data.Maps.GMapNPC", "_positions");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.APIClient.Request.LoginInfo", "Email");
        AssertPresent(census, NON_PUBLIC_PROPERTY, "AL.APIClient.Request.LoginInfo", "Password");

        // The five private *fields* the plan names.
        AssertPresent(census, FIELD, "AL.Data.Conditions.GCondition", "_canMove");
        AssertPresent(census, FIELD, "AL.Data.Maps.GMapNPC", "_name");
        AssertPresent(census, FIELD, "AL.Data.Monsters.GMonsterAbility", "_amount");
        AssertPresent(census, FIELD, "AL.Data.Monsters.GMonsterAbility", "_damage");
        AssertPresent(census, FIELD, "AL.Data.Monsters.GMonsterAbility", "_heal");
    }

    [TestMethod]
    public void T11_StartFrame_BindsNonPublicSetters()
    {
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;
        start.Should().NotBeNull();

        // EntityBase (protected set)
        start.Map.Should().Be("main");

        // In carries no [JsonProperty] (the attribute above it belongs to Id), so it is absent from the census
        // and stays null even though the wire sends "in":"main". Pinned per decision 9 / S21.
        start.In.Should().BeNull("EntityBase.In is deliberately unbound under Newtonsoft today");
        start.MaxHP.Should().Be(7826f);
        start.Level.Should().Be(54);
        start.X.Should().BeApproximately(40.40316655490353f, 1e-3f);
        start.Y.Should().BeApproximately(541.1426134776386f, 1e-3f);
        start.Conditions.Should().ContainSingle("the start frame carries exactly the mluck condition");

        // Player (protected set)
        start.AFK.Should().BeTrue();
        start.Age.Should().Be(69);
        start.Class.ToString().Should().Be("Merchant");
        start.MaxMP.Should().Be(2060f);
        start.Owner.Should().Be("1234");
        start.Skin.Should().Be("sarmor2c");
        start.Cosmetics.Should().NotBeNull();

        // Strengthened from NotBeEmpty(): the frame sends only the 20 populated slots, and Player.OnDeserialized
        // back-fills every remaining Slot with null. A converter that skips the callback still leaves 20 non-empty
        // slots here, which is how the earlier pin passed while every unequipped indexer read was throwing.
        start.Slots.Should().HaveCount(Enum.GetValues<Slot>().Length, "OnDeserialized back-fills every missing slot");
        start.Slots.Values.Should().Contain(slotItem => slotItem != null, "the character has equipped items");

        // Character (protected set, plus Inventory internal set)
        start.CodeCost.Should().Be(4f);
        start.EmptySlots.Should().Be(26);
        start.ExtraRange.Should().Be(25f);
        start.InventorySize.Should().Be(42);
        start.MPCost.Should().Be(65);
        start.Tax.Should().BeApproximately(0.03f, 1e-4f);
        start.Friends.Should().ContainSingle().Which.Should().Be("6341309189586944");
        start.OwnedCosmetics.Should().HaveCount(30);
        start.Inventory.Should().NotBeNull();
        start.Inventory.Count.Should().Be(42, "the frame's items array carries 42 entries");

        // Count is the wire array length, never the sized capacity, so the old "capacity is set from InventorySize"
        // reason on it was reading the wrong number. Capacity is the real one, and the downcast is load-bearing:
        // SetCapacity casts the backing store to List<Item?>. This frame's array is already InventorySize long so
        // SetCapacity is a no-op on it — PopulateConverterCharacterization pins the short-array case that moves it.
        start.Inventory.Items.Should().BeOfType<List<Item?>>().Which.Capacity.Should().BeGreaterThanOrEqualTo(start.InventorySize);
        start.Code.Should().NotBeNullOrEmpty("the start frame carries the account's load_code string");
    }

    [TestMethod]
    public void T11_PlayerAndEntitiesFrames_BindEntityAndPlayerSetters()
    {
        var player = TestJson.Socket<Player>(ReadFrame(PLAYER_FRAME))!;
        player.Should().NotBeNull();

        player.Focus.Should().Be("kouin", "the player-frame Focus is only reachable off an entities player");
        player.Owner.Should().Be("6314512450322432");
        player.Class.ToString().Should().Be("Merchant");
        player.Age.Should().Be(338);
        player.PDPS.Should().BeApproximately(17.218536885677565f, 1e-3f);
        player.Skin.Should().Be("marmor12a");
        player.Moving.Should().BeTrue();

        var entities = TestJson.Socket<EntitiesData>(ReadFrame(ENTITIES_FRAME))!;
        entities.Monsters.Should().NotBeEmpty();

        var monster = entities.Monsters.First();

        // Raw deserialization does not stamp the envelope-level map/in onto child entities — ALClient does that
        // later. Pin it so the migration preserves the current (null) shape.
        monster.Map.Should().BeNull("the frame envelope map is not applied to child entities during deserialization");
        monster.MaxHP.Should().Be(86400f);
        monster.Level.Should().Be(8);
        monster.MoveNum.Should().Be(22612448UL);
        monster.Angle.Should().BeApproximately(78.20491891313337f, 1e-3f);
        monster.GoingX.Should().BeApproximately(363.95981679423096f, 1e-3f);
        monster.GoingY.Should().BeApproximately(-1874.8896953933113f, 1e-3f);

        // NPCName is reachable only from an entities player carrying "npc"; the start frame has one ("pvp").
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;
        var npcPlayer = start.Entities.Players.Single(candidate => candidate.NPCName != null);
        npcPlayer.NPCName.Should().Be("pvp");
    }

    [TestMethod]
    public void T11_FrameCoverage_ReportsWhichSettersNoPayloadExercises()
    {
        var census = BuildCensus();
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;
        var entities = TestJson.Socket<EntitiesData>(ReadFrame(ENTITIES_FRAME))!;
        var player = TestJson.Socket<Player>(ReadFrame(PLAYER_FRAME))!;

        var probes = CollectProbes(start).Concat(CollectProbes(entities)).Concat(CollectProbes(player)).ToList();

        var frameReachable = census.Where(entry => entry.Member is PropertyInfo property
                                                   && probes.Any(probe => property.DeclaringType!.IsInstanceOfType(probe)))
                                    .ToList();

        var covered = new List<string>();
        var uncovered = new List<string>();

        foreach (var entry in frameReachable)
        {
            var property = (PropertyInfo)entry.Member;

            // A member is covered if ANY frame's entity drives it away from its default.
            var isCovered = probes.Where(probe => property.DeclaringType!.IsInstanceOfType(probe))
                                  .Select(probe => property.GetValue(probe))
                                  .Any(value => IsNonDefault(value, property.PropertyType));

            var label = $"{property.DeclaringType!.Name}.{property.Name}";

            if (isCovered)
            {
                covered.Add(label);

                continue;
            }

            uncovered.Add(label);
        }

        TestContext.WriteLine($"frame-reachable non-public setters : {frameReachable.Count}");
        TestContext.WriteLine($"covered (non-default in a frame)   : {covered.Count}");
        TestContext.WriteLine($"uncovered (default in every frame) : {uncovered.Count}");
        TestContext.WriteLine("UNCOVERED: " + string.Join(", ", uncovered.OrderBy(name => name)));

        // Only non-public setters live on the entity frame types; every field and non-public property in the
        // census is on static game data (GItem/GSkill/GMapNPC/...), which no socket frame reaches.
        frameReachable.Should().OnlyContain(entry => entry.Category == NON_PUBLIC_SETTER);

        // 89 of the 105 instance non-public setters live on EntityBase/Player/Character (including the attributed
        // stat setters now decorated on AttributedObjectBase after the Phase 1 move); the other 16 are on
        // GGeometry/GDoor/GSkill/BossInfo (static/boss data, not an entity frame).
        // Phase 7 moved Emotion/Friends/OwnedCosmetics off Character onto ALClient (not an entity frame type),
        // dropping frame-reachable 92 -> 89 and covered 59 -> 56; MaxMP/Focus merely relocated Player -> EntityBase.
        // Phase 11 added eight Character owner-stats (an entity frame type): frame-reachable 89 -> 97.
        frameReachable.Should().HaveCount(97);
        // Phase 11: five of the eight new Character stats (max_xp/goldm/xpm/luckm/cash) are non-default in the
        // captured start frame; incdmgamp/mcourage/pcourage stay default. covered 56 -> 61.
        covered.Should().HaveCount(61, "the start/player/entities frames drive 61 setters away from their defaults");

        // The 36 setters no captured frame exercises to a non-default value. These are the members a
        // migration reviewer must eyeball by hand — no value assertion can guard them here. The 23
        // AttributedObjectBase stats are attributed setters the frames happen to leave at zero.
        uncovered.Should()
                 .BeEquivalentTo(
                     "AttributedObjectBase.Awesomeness",
                     "AttributedObjectBase.Blast",
                     "AttributedObjectBase.Bling",
                     "AttributedObjectBase.Charisma",
                     "AttributedObjectBase.Crit",
                     "AttributedObjectBase.CritDamage",
                     "AttributedObjectBase.Cuteness",
                     "AttributedObjectBase.DReturn",
                     "AttributedObjectBase.Explosion",
                     "AttributedObjectBase.FrequencyMod",
                     "AttributedObjectBase.GoldSteal",
                     "AttributedObjectBase.HealMod",
                     "AttributedObjectBase.Lifesteal",
                     "AttributedObjectBase.Luck",
                     "AttributedObjectBase.ManaSteal",
                     "AttributedObjectBase.Miss",
                     "AttributedObjectBase.MPCost",
                     "AttributedObjectBase.Output",
                     "AttributedObjectBase.PoisonResistance",
                     "AttributedObjectBase.PotionsMod",
                     "AttributedObjectBase.Reflection",
                     "AttributedObjectBase.Stat",
                     "AttributedObjectBase.StunChance",
                     "Character.AggroTargets",
                     "Character.Bank",
                     "Character.Cache",
                     "Character.Fear",
                     "Character.MapChangeCount",
                     // Phase 11: three of the eight new Character stats stay default in every captured frame
                     "Character.IncomingDamageAmp",
                     "Character.MCourage",
                     "Character.PCourage",
                     "EntityBase.ABS",
                     "Player.Controller",
                     "Player.RIP",
                     "Player.Stand",
                     "Player.Teleporting");
    }

    #region census construction
    private static IReadOnlyList<CensusEntry> BuildCensus()
    {
        var entries = new List<CensusEntry>();

        foreach (var assembly in LibraryAssemblies)
        foreach (var type in GetTypes(assembly))
        {
            const BindingFlags flags = BindingFlags.Public
                                       | BindingFlags.NonPublic
                                       | BindingFlags.Instance
                                       | BindingFlags.Static
                                       | BindingFlags.DeclaredOnly;

            foreach (var member in type.GetMembers(flags))
            {
                if (member is not (PropertyInfo or FieldInfo))
                    continue;

                // Newtonsoft attribute only; the STJ side is what the migration will add.
                var jsonProperty = member.GetCustomAttribute<JsonPropertyAttribute>(inherit: false);

                if (jsonProperty is null)
                    continue;

                if (!TryClassify(member, out var category, out var isStatic))
                    continue;

                entries.Add(new CensusEntry(
                    category,
                    isStatic,
                    assembly.GetName().Name!,
                    type.FullName ?? type.Name,
                    member.Name,
                    jsonProperty.PropertyName ?? string.Empty,
                    member));
            }
        }

        return entries;
    }

    /// <summary>
    ///     Classifies a <c>[JsonProperty]</c> member into the three categories STJ needs <c>[JsonInclude]</c> for.
    ///     Returns false for members STJ binds on its own (public setter, public <c>init</c>, or get-only).
    /// </summary>
    private static bool TryClassify(MemberInfo member, out string category, out bool isStatic)
    {
        if (member is FieldInfo field)
        {
            category = FIELD;
            isStatic = field.IsStatic;

            // Only instance fields are [JsonInclude] targets. Enum members are static literal fields, and the
            // ~48 [JsonProperty] on BankPack members are dead decoration Newtonsoft ignores — excluded here and
            // counted separately by T11_Census_PinsCategoryCounts.
            return !field.IsStatic;
        }

        var property = (PropertyInfo)member;
        var getter = property.GetGetMethod(nonPublic: true);
        var setter = property.GetSetMethod(nonPublic: true);

        var propertyIsPublic = (getter?.IsPublic ?? false) || (setter?.IsPublic ?? false);
        isStatic = (getter ?? setter)?.IsStatic ?? false;

        if (!propertyIsPublic)
        {
            category = NON_PUBLIC_PROPERTY;

            return true;
        }

        // init on a public property reports IsPublic == true, so it correctly falls through here.
        if (setter is { IsPublic: false })
        {
            category = NON_PUBLIC_SETTER;

            return true;
        }

        category = string.Empty;

        return false;
    }

    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        // A referenced dependency that fails to load would throw here; keep the types that did load.
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }

    private static string Render(IReadOnlyList<CensusEntry> census)
    {
        var lines = census.Select(entry => string.Join(
                              '\t',
                              entry.Category,
                              entry.IsStatic ? "static" : "instance",
                              entry.Assembly,
                              entry.TypeFullName,
                              entry.MemberName,
                              entry.JsonName))
                          .OrderBy(line => line, StringComparer.Ordinal);

        return string.Join('\n', lines);
    }

    private static string Normalize(string text)
        => text.Replace("\r\n", "\n")
               .Trim('\n');

    private static void AssertPresent(IReadOnlyList<CensusEntry> census, string category, string typeFullName, string memberName)
        => census.Should()
                 .ContainSingle(entry => entry.Category == category
                                         && entry.TypeFullName == typeFullName
                                         && entry.MemberName == memberName,
                     $"{typeFullName}.{memberName} must be in the census as {category}");
    #endregion

    #region frame coverage helpers
    private static string ReadFrame(string fileName)
    {
        var json = Fixture.ReadCommittedSnapshot(fileName);

        json.Should()
            .NotBeNull($"the frame fixture must be committed at AL.Tests/Fixtures/snapshots/{fileName}");

        return json!;
    }

    /// <summary>
    ///     The deserialized object plus every entity nested inside it, so a census member on any entity type
    ///     is read off a real instance.
    /// </summary>
    private static IEnumerable<object> CollectProbes(object root)
    {
        yield return root;

        switch (root)
        {
            case StartData start when start.Entities is not null:
                foreach (var probe in CollectProbes(start.Entities))
                    yield return probe;

                break;
            case EntitiesData entities:
                foreach (var probe in entities.Players.Cast<object>().Concat(entities.Monsters))
                    yield return probe;

                break;
        }
    }

    private static bool IsNonDefault(object? value, Type type)
    {
        if (value is null)
            return false;

        if (!type.IsValueType)
            return true;

        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        var defaultValue = Activator.CreateInstance(underlying);

        return !value.Equals(defaultValue);
    }
    #endregion

    private sealed record CensusEntry(
        string Category,
        bool IsStatic,
        string Assembly,
        string TypeFullName,
        string MemberName,
        string JsonName,
        MemberInfo Member);
}
