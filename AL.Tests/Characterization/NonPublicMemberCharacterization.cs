#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using AL.APIClient.Request;
using AL.Client;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Data;
using AL.Pathfinding;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     T11 — Non-public member census. Pins every member System.Text.Json would silently bind to its default if its
///     <c>
///         [JsonInclude]
///     </c>
///     /
///     <c>
///         [JsonPropertyName]
///     </c>
///     decoration were removed.
/// </summary>
/// <remarks>
///     The census is built by reflection so it cannot drift out of sync with the code: any newly added non-public JSON
///     member changes the committed fixture and fails <see cref="T11_Census_MatchesCommittedFixture" />. The value
///     assertions then prove that a real
///     <c>
///         start
///     </c>
///     /
///     <c>
///         player
///     </c>
///     /
///     <c>
///         entities
///     </c>
///     frame actually drives those members away from their defaults under the current serializer.
///     <br />
///     Phase 6b re-pointed the predicate from the old serializer's property attribute onto the two STJ attributes; every
///     pinned count in <see cref="T11_Census_PinsCategoryCounts" /> was re-derived, not carried over.
/// </remarks>
[NotInParallel(ParallelKeys.GAME_DATA)]
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
    ///     The six library assemblies whose types the migration decorates. Resolved through one representative public type per
    ///     assembly so a rename fails to compile rather than silently narrowing the census.
    /// </summary>
    private static readonly IReadOnlyList<Assembly> LibraryAssemblies = new[]
        {
            typeof(AttributedObjectBase).Assembly, // AL.Core
            typeof(GameData).Assembly, // AL.Data
            typeof(ALClient).Assembly, // AL.Client
            typeof(StartData).Assembly, // AL.SocketClient
            typeof(LoginInfo).Assembly, // AL.APIClient
            typeof(Pathfinder).Assembly // AL.Pathfinding
        }.Distinct()
         .ToArray();

    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    ///     Every instance property in the six assemblies whose property is public but whose set accessor is non-public — the
    ///     true [JsonInclude] target set, independent of whether the member carries any serialization attribute at all.
    /// </summary>
    private static int AllInstanceNonPublicSetterCount()
    {
        var count = 0;

        foreach (var assembly in LibraryAssemblies)
            foreach (var type in GetTypes(assembly))
                foreach (var property in type.GetProperties(
                             BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var getter = property.GetGetMethod(true);
                    var setter = property.GetSetMethod(true);
                    var propertyIsPublic = (getter?.IsPublic ?? false) || (setter?.IsPublic ?? false);

                    if (propertyIsPublic && setter is { IsPublic: false })
                        count++;
                }

        return count;
    }

    [Test]
    public void T11_Census_ContainsEveryNamedSite()
    {
        var census = BuildCensus();

        // The eight non-public *properties* the plan names (property visibility itself is non-public).
        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Skills.GSkill",
            "ListTargets");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Skills.GSkill",
            "ReuseCooldown");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Maps.GMapMonster",
            "_boundaries");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Maps.GMapMonster",
            "_boundary");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Maps.GMapNPC",
            "_position");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.Data.Maps.GMapNPC",
            "_positions");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.APIClient.Request.LoginInfo",
            "Email");

        AssertPresent(
            census,
            NON_PUBLIC_PROPERTY,
            "AL.APIClient.Request.LoginInfo",
            "Password");

        // The five private *fields* the plan names.
        AssertPresent(
            census,
            FIELD,
            "AL.Data.Conditions.GCondition",
            "_canMove");

        AssertPresent(
            census,
            FIELD,
            "AL.Data.Maps.GMapNPC",
            "_name");

        AssertPresent(
            census,
            FIELD,
            "AL.Data.Monsters.GMonsterAbility",
            "_amount");

        AssertPresent(
            census,
            FIELD,
            "AL.Data.Monsters.GMonsterAbility",
            "_damage");

        AssertPresent(
            census,
            FIELD,
            "AL.Data.Monsters.GMonsterAbility",
            "_heal");
    }

    [Test]
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

        Normalize(committed)
            .Should()
            .Be(
                Normalize(rendered),
                "the non-public JSON member census drifted; re-commit the generated snapshot if the change is intended");
    }

    [Test]
    public void T11_Census_PinsCategoryCounts()
    {
        var census = BuildCensus();

        var instanceNonPublicSetters = census.Count(entry => entry is { Category: NON_PUBLIC_SETTER, IsStatic: false });
        var staticNonPublicSetters = census.Count(entry => entry is { Category: NON_PUBLIC_SETTER, IsStatic: true });
        var nonPublicProperties = census.Count(entry => entry.Category == NON_PUBLIC_PROPERTY);
        var fields = census.Count(entry => entry.Category == FIELD);

        // Diagnostic for the migration hazard the census surfaces: the attribute-gated blind spot.
        var allInstanceNonPublicSetters = AllInstanceNonPublicSetterCount();
        var auditGap = allInstanceNonPublicSetters - instanceNonPublicSetters;

        Console.WriteLine($"instance non-public setters (w/ STJ attribute) : {instanceNonPublicSetters}");
        Console.WriteLine($"static  non-public setters (GameData)          : {staticNonPublicSetters}");
        Console.WriteLine($"non-public properties                          : {nonPublicProperties}");
        Console.WriteLine($"instance fields                                : {fields}");
        Console.WriteLine($"total census entries                           : {census.Count}");
        Console.WriteLine($"ALL instance non-public setters (any/no attr)  : {allInstanceNonPublicSetters}");
        Console.WriteLine($"STJ-attribute-gated audit BLIND SPOT           : {auditGap}");

        // Pinned to the REAL reflection result, re-derived member-by-member after Phase 6b re-pointed the
        // predicate from [JsonProperty] onto [JsonInclude]/[JsonPropertyName]. The two populations are not the
        // same set, so every number below was recomputed rather than carried across:
        //   instance non-public setters : 113 -> 113  (unchanged)
        //   static  non-public setters  :  20 ->   0  (see below — the whole category emptied)
        //   non-public properties       :   8 ->   8  (unchanged)
        //   instance fields             :   5 ->   5  (unchanged)
        // The three unchanged categories are 1:1 replacements: each [JsonProperty] on a non-public member became
        // [JsonInclude], and each [JsonProperty("wire")] additionally kept its name as [JsonPropertyName("wire")],
        // so the same 126 members are still named with the same wire keys.
        instanceNonPublicSetters.Should()
                                .Be(113);

        // The one real re-baseline. The 20 GameData roots left the census entirely: STJ cannot bind static
        // members at all, so they are no longer driven by a serialization attribute but by [GameDataRoot], which
        // GameData.Bind reflects over by hand. Nothing went uncovered with them — GameDataLoadCharacterization's
        // T1_StaticScalars_Bind / T1_Datums_FullyPopulated assert those roots by value, which is a stronger
        // guard than a name in this census ever was.
        staticNonPublicSetters.Should()
                              .Be(0);

        nonPublicProperties.Should()
                           .Be(8);

        fields.Should()
              .Be(5);

        // The ~48 dead [JsonProperty] that used to sit on BankPack (and a few other) enum members had their own
        // count pinned at 0 here. Phase 6c dropped that guard along with the package reference it needed: with
        // the old serializer gone from the solution entirely, the attribute cannot be reapplied without
        // reintroducing the dependency, which the compiler now rejects. That is a stronger guard than a count.

        // Finding, and the reason an attribute-gated audit is unsafe under STJ exactly as it was under the old
        // serializer: 150 instance properties in the six assemblies have a non-public setter, and only 113 carry
        // [JsonInclude]/[JsonPropertyName], so an audit keyed on the attribute is blind to 37 of them — e.g.
        // EntityBase.In (the server sends "in", nothing binds it), EntityBase.PresentFields, the
        // Emotion/Friends/OwnedCosmetics setters that moved onto ALClient, and ALSocketClient.LastDisconnectReason
        // / ALClient.FatalError, all populated imperatively. Not missed bindings, but exactly what an
        // attribute-keyed audit cannot see. Key on "non-public accessor", never on "has an attribute".
        auditGap.Should()
                .Be(37);

        // Attribute-independent by construction, so the re-point could not move it: 150, unchanged.
        allInstanceNonPublicSetters.Should()
                                   .Be(150);
    }

    [Test]
    public void T11_FrameCoverage_ReportsWhichSettersNoPayloadExercises()
    {
        var census = BuildCensus();
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;
        var entities = TestJson.Socket<EntitiesData>(ReadFrame(ENTITIES_FRAME))!;
        var player = TestJson.Socket<Player>(ReadFrame(PLAYER_FRAME))!;

        var probes = CollectProbes(start)
                     .Concat(CollectProbes(entities))
                     .Concat(CollectProbes(player))
                     .ToList();

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

        Console.WriteLine($"frame-reachable non-public setters : {frameReachable.Count}");
        Console.WriteLine($"covered (non-default in a frame)   : {covered.Count}");
        Console.WriteLine($"uncovered (default in every frame) : {uncovered.Count}");
        Console.WriteLine("UNCOVERED: " + string.Join(", ", uncovered.OrderBy(name => name)));

        // Only non-public setters live on the entity frame types; every field and non-public property in the
        // census is on static game data (GItem/GSkill/GMapNPC/...), which no socket frame reaches.
        frameReachable.Should()
                      .OnlyContain(entry => entry.Category == NON_PUBLIC_SETTER);

        // 89 of the 105 instance non-public setters live on EntityBase/Player/Character (including the attributed
        // stat setters now decorated on AttributedObjectBase after the Phase 1 move); the other 16 are on
        // GGeometry/GDoor/GSkill/BossInfo (static/boss data, not an entity frame).
        // Phase 7 moved Emotion/Friends/OwnedCosmetics off Character onto ALClient (not an entity frame type),
        // dropping frame-reachable 92 -> 89 and covered 59 -> 56; MaxMP/Focus merely relocated Player -> EntityBase.
        // Phase 11 added eight Character owner-stats (an entity frame type): frame-reachable 89 -> 97.
        frameReachable.Should()
                      .HaveCount(97);

        // Phase 11: five of the eight new Character stats (max_xp/goldm/xpm/luckm/cash) are non-default in the
        // captured start frame; incdmgamp/mcourage/pcourage stay default. covered 56 -> 61.
        covered.Should()
               .HaveCount(61, "the start/player/entities frames drive 61 setters away from their defaults");

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

    /// <summary>
    ///     An item's
    ///     <c>
    ///         expires
    ///     </c>
    ///     must bind in the shapes the server actually sends.
    /// </summary>
    /// <remarks>
    ///     The server writes it with JavaScript's
    ///     <c>
    ///         Date.prototype.toUTCString()
    ///     </c>
    ///     — RFC 1123, not ISO 8601 — and substitutes an empty string when the item has no expiry (
    ///     <c>
    ///         js/common_functions.js
    ///     </c>
    ///     :
    ///     <c>
    ///         attributes.expires = attributes.expires ? attributes.expires.toUTCString() : ''
    ///     </c>
    ///     ). The old serializer's ISO date converter absorbed both shapes; System.Text.Json's built-in reader accepts only
    ///     ISO 8601 and throws on either, which inside a socket frame discards the entire frame rather than one field — hence
    ///     the custom converter these assertions pin.
    /// </remarks>
    [Test]
    public void T11_ItemExpires_BindsEveryShapeTheServerSends()
    {
        //RFC 1123, exactly what toUTCString() produces
        var rfc1123 = TestJson.Socket<Item>(@"{""name"":""goldbooster"",""expires"":""Wed, 14 Jun 2017 07:00:00 GMT""}")!;

        rfc1123.Expires
               .Should()
               .Be(
                   new DateTime(
                       2017,
                       6,
                       14,
                       7,
                       0,
                       0,
                       DateTimeKind.Utc));

        //the no-expiry placeholder
        TestJson.Socket<Item>(@"{""name"":""hpot0"",""expires"":""""}")!.Expires
                .Should()
                .BeNull("an empty string is the server's way of saying the item does not expire");

        TestJson.Socket<Item>(@"{""name"":""hpot0"",""expires"":null}")!.Expires
                .Should()
                .BeNull();

        //an ISO value must still take the built-in fast path unchanged
        TestJson.Socket<Item>(@"{""name"":""elixirluck"",""expires"":""2017-06-14T07:00:00Z""}")!.Expires
                .Should()
                .Be(
                    new DateTime(
                        2017,
                        6,
                        14,
                        7,
                        0,
                        0,
                        DateTimeKind.Utc));

        //the same member on the equipped-slot shape, which declares Expires separately
        TestJson.Socket<SlotItem>(@"{""name"":""goldbooster"",""expires"":""Wed, 14 Jun 2017 07:00:00 GMT""}")!.Expires
                .Should()
                .Be(
                    new DateTime(
                        2017,
                        6,
                        14,
                        7,
                        0,
                        0,
                        DateTimeKind.Utc));
    }

    [Test]
    public void T11_PlayerAndEntitiesFrames_BindEntityAndPlayerSetters()
    {
        var player = TestJson.Socket<Player>(ReadFrame(PLAYER_FRAME))!;

        player.Should()
              .NotBeNull();

        player.Focus
              .Should()
              .Be("kouin", "the player-frame Focus is only reachable off an entities player");

        player.Owner
              .Should()
              .Be("6314512450322432");

        player.Class
              .ToString()
              .Should()
              .Be("Merchant");

        player.Age
              .Should()
              .Be(338);

        player.PDPS
              .Should()
              .BeApproximately(17.218536885677565f, 1e-3f);

        player.Skin
              .Should()
              .Be("marmor12a");

        player.Moving
              .Should()
              .BeTrue();

        var entities = TestJson.Socket<EntitiesData>(ReadFrame(ENTITIES_FRAME))!;

        entities.Monsters
                .Should()
                .NotBeEmpty();

        var monster = entities.Monsters.First();

        // Raw deserialization does not stamp the envelope-level map/in onto child entities — ALClient does that
        // later. Pin it so the migration preserves the current (null) shape.
        monster.Map
               .Should()
               .BeNull("the frame envelope map is not applied to child entities during deserialization");

        monster.MaxHP
               .Should()
               .Be(86400f);

        monster.Level
               .Should()
               .Be(8);

        monster.MoveNum
               .Should()
               .Be(22612448UL);

        monster.Angle
               .Should()
               .BeApproximately(78.20491891313337f, 1e-3f);

        monster.GoingX
               .Should()
               .BeApproximately(363.95981679423096f, 1e-3f);

        monster.GoingY
               .Should()
               .BeApproximately(-1874.8896953933113f, 1e-3f);

        // NPCName is reachable only from an entities player carrying "npc"; the start frame has one ("pvp").
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;
        var npcPlayer = start.Entities.Players.Single(candidate => candidate.NPCName != null);

        npcPlayer.NPCName
                 .Should()
                 .Be("pvp");
    }

    [Test]
    public void T11_StartFrame_BindsNonPublicSetters()
    {
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;

        start.Should()
             .NotBeNull();

        // EntityBase (protected set)
        start.Map
             .Should()
             .Be("main");

        // In carries no serialization attribute at all, so it is absent from the census, and STJ skips a
        // protected setter without [JsonInclude] — it stays null even though the wire sends "in":"main".
        // Pinned per decision 9 / S21.
        start.In
             .Should()
             .BeNull("EntityBase.In is deliberately left unbound");

        start.MaxHP
             .Should()
             .Be(7826f);

        start.Level
             .Should()
             .Be(54);

        start.X
             .Should()
             .BeApproximately(40.40316655490353f, 1e-3f);

        start.Y
             .Should()
             .BeApproximately(541.1426134776386f, 1e-3f);

        start.Conditions
             .Should()
             .ContainSingle("the start frame carries exactly the mluck condition");

        // Player (protected set)
        start.AFK
             .Should()
             .BeTrue();

        start.Age
             .Should()
             .Be(69);

        start.Class
             .ToString()
             .Should()
             .Be("Merchant");

        start.MaxMP
             .Should()
             .Be(2060f);

        start.Owner
             .Should()
             .Be("1234");

        start.Skin
             .Should()
             .Be("sarmor2c");

        start.Cosmetics
             .Should()
             .NotBeNull();

        // Strengthened from NotBeEmpty(): the frame sends only the 20 populated slots, and Player.OnDeserialized
        // back-fills every remaining Slot with null. A converter that skips the callback still leaves 20 non-empty
        // slots here, which is how the earlier pin passed while every unequipped indexer read was throwing.
        start.Slots
             .Should()
             .HaveCount(
                 Enum.GetValues<Slot>()
                     .Length,
                 "OnDeserialized back-fills every missing slot");

        start.Slots
             .Values
             .Should()
             .Contain(slotItem => slotItem != null, "the character has equipped items");

        // Character (protected set, plus Inventory internal set)
        start.CodeCost
             .Should()
             .Be(4f);

        start.EmptySlots
             .Should()
             .Be(26);

        start.ExtraRange
             .Should()
             .Be(25f);

        start.InventorySize
             .Should()
             .Be(42);

        start.MPCost
             .Should()
             .Be(65);

        start.Tax
             .Should()
             .BeApproximately(0.03f, 1e-4f);

        start.Friends
             .Should()
             .ContainSingle()
             .Which
             .Should()
             .Be("6341309189586944");

        start.OwnedCosmetics
             .Should()
             .HaveCount(30);

        start.Inventory
             .Should()
             .NotBeNull();

        start.Inventory
             .Count
             .Should()
             .Be(42, "the frame's items array carries 42 entries");

        // Count is the wire array length, never the sized capacity, so the old "capacity is set from InventorySize"
        // reason on it was reading the wrong number. Capacity is the real one, and the downcast is load-bearing:
        // SetCapacity casts the backing store to List<Item?>. This frame's array is already InventorySize long so
        // SetCapacity is a no-op on it — PopulateConverterCharacterization pins the short-array case that moves it.
        start.Inventory
             .Items
             .Should()
             .BeOfType<List<Item?>>()
             .Which
             .Capacity
             .Should()
             .BeGreaterThanOrEqualTo(start.InventorySize);

        start.Code
             .Should()
             .NotBeNullOrEmpty("the start frame carries the account's load_code string");
    }

    /// <summary>
    ///     The login frame's
    ///     <c>
    ///         s_info
    ///     </c>
    ///     must arrive with its bosses, not just its event flags.
    /// </summary>
    /// <remarks>
    ///     <c>
    ///         BossInfo
    ///     </c>
    ///     is
    ///     <c>
    ///         [JsonIgnore]
    ///     </c>
    ///     with a get-only initializer, so only
    ///     <c>
    ///         EventAndBossDataConverter
    ///     </c>
    ///     ever fills it — and that converter is a
    ///     <c>
    ///         JsonConverter&lt;EventAndBossData&gt;
    ///     </c>
    ///     , which does not claim a member declared as the base
    ///     <c>
    ///         EventAndBossInfo
    ///     </c>
    ///     . Declared as the base, this frame bound the four event flags and silently dropped all three live bosses, leaving
    ///     <c>
    ///         ALClient.EventsAndBosses
    ///     </c>
    ///     empty from login until the first periodic
    ///     <c>
    ///         server_info
    ///     </c>
    ///     push happened to refill it.
    /// </remarks>
    [Test]
    public void T11_StartFrame_SInfo_CarriesBossesNotJustEventFlags()
    {
        var start = TestJson.Socket<StartData>(ReadFrame(START_FRAME))!;

        start.EventAndBossInfo
             .HolidaySeason
             .Should()
             .BeTrue("the captured frame's s_info carries holidayseason");

        start.EventAndBossInfo
             .BossInfo
             .Should()
             .ContainKeys("icegolem", "snowman", "franky");

        start.EventAndBossInfo
             .BossInfo["franky"]
             .MaxHP
             .Should()
             .Be(120000000f);
    }

    private sealed record CensusEntry(
        string Category,
        bool IsStatic,
        string Assembly,
        string TypeFullName,
        string MemberName,
        string JsonName,
        MemberInfo Member);

    #region census construction
    private static IReadOnlyList<CensusEntry> BuildCensus()
    {
        var entries = new List<CensusEntry>();

        foreach (var assembly in LibraryAssemblies)
            foreach (var type in GetTypes(assembly))
            {
                const BindingFlags FLAGS = BindingFlags.Public
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Instance
                                           | BindingFlags.Static
                                           | BindingFlags.DeclaredOnly;

                foreach (var member in type.GetMembers(FLAGS))
                {
                    if (member is not (PropertyInfo or FieldInfo))
                        continue;

                    // The two STJ attributes that make a member census-relevant: [JsonInclude] opts a member the
                    // serializer would otherwise skip back in, [JsonPropertyName] renames it. Either alone counts —
                    // a renamed public-init member carries only the latter, an un-renamed protected setter only the
                    // former, and most of the entity frame types carry both.
                    var jsonInclude = member.GetCustomAttribute<JsonIncludeAttribute>(false);
                    var jsonPropertyName = member.GetCustomAttribute<JsonPropertyNameAttribute>(false);

                    if (jsonInclude is null && jsonPropertyName is null)
                        continue;

                    if (!TryClassify(member, out var category, out var isStatic))
                        continue;

                    entries.Add(
                        new CensusEntry(
                            category,
                            isStatic,
                            assembly.GetName()
                                    .Name!,
                            type.FullName ?? type.Name,
                            member.Name,
                            jsonPropertyName?.Name ?? string.Empty,
                            member));
                }
            }

        return entries;
    }

    /// <summary>
    ///     Classifies an attributed member into the three categories STJ needs
    ///     <c>
    ///         [JsonInclude]
    ///     </c>
    ///     for. Returns false for members STJ binds on its own (public setter, public
    ///     <c>
    ///         init
    ///     </c>
    ///     , or get-only).
    /// </summary>
    private static bool TryClassify(MemberInfo member, out string category, out bool isStatic)
    {
        if (member is FieldInfo field)
        {
            category = FIELD;
            isStatic = field.IsStatic;

            // Only instance fields are [JsonInclude] targets. Enum members are static literal fields no
            // serializer ever binds, so they are excluded here.
            return !field.IsStatic;
        }

        var property = (PropertyInfo)member;
        var getter = property.GetGetMethod(true);
        var setter = property.GetSetMethod(true);

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
        } catch (ReflectionTypeLoadException ex)
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

    private static void AssertPresent(
        IReadOnlyList<CensusEntry> census,
        string category,
        string typeFullName,
        string memberName)
        => census.Should()
                 .ContainSingle(
                     entry => (entry.Category == category) && (entry.TypeFullName == typeFullName) && (entry.MemberName == memberName),
                     $"{typeFullName}.{memberName} must be in the census as {category}");
    #endregion

    #region frame coverage helpers
    private static string ReadFrame(string fileName)
    {
        var json = Fixture.ReadCommittedSnapshot(fileName);

        json.Should()
            .NotBeNull($"the frame fixture must be committed at AL.Tests/Fixtures/snapshots/{fileName}");

        return json;
    }

    /// <summary>
    ///     The deserialized object plus every entity nested inside it, so a census member on any entity type is read off a
    ///     real instance.
    /// </summary>
    private static IEnumerable<object> CollectProbes(object root)
    {
        yield return root;

        switch (root)
        {
            //Entities is `= null!` and only ever filled by deserialization, so a frame can genuinely arrive without it
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            case StartData start when start.Entities is not null:
                foreach (var probe in CollectProbes(start.Entities))
                    yield return probe;

                break;
            case EntitiesData entities:
                foreach (var probe in entities.Players
                                              .Cast<object>()
                                              .Concat(entities.Monsters))
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
}