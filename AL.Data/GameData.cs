#region
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.Core.Json;
using AL.Data.Achievements;
using AL.Data.Classes;
using AL.Data.Conditions;
using AL.Data.Craft;
using AL.Data.Dimensions;
using AL.Data.Dismantle;
using AL.Data.Drops;
using AL.Data.Events;
using AL.Data.Games;
using AL.Data.Geometry;
using AL.Data.Images;
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Monsters;
using AL.Data.Multipliers;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Sets;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.Data.Tokens;
using Chaos.Extensions.Common;
using Common.Logging;
using JetBrains.Annotations;
#endregion

//the G-data statics are written only by Bind's reflection, which requires GetSetMethod(true) to be non-null
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AL.Data;

public record GameData
{
    /// <summary>
    ///     The npc every exchange with no quest tag of its own is measured against -
    ///     <c>
    ///         G.maps.main.exchange
    ///     </c>
    ///     , which the map data carries as a placement of this id on
    ///     <c>
    ///         main
    ///     </c>
    ///     alone. The copy on
    ///     <c>
    ///         original_main
    ///     </c>
    ///     sits on a map marked ignored, so <see cref="EnrichNPCs" /> never adds it to
    ///     <c>
    ///         Locations
    ///     </c>
    ///     - the same skip the server's own placement loop takes (
    ///     <c>
    ///         js/old_common_functions.js:197
    ///     </c>
    ///     , filling
    ///     <c>
    ///         map.exchange
    ///     </c>
    ///     at
    ///     <c>
    ///         :236
    ///     </c>
    ///     ).
    /// </summary>
    private const string EXCHANGE_NPC = "exchange";

    /// <summary>
    ///     The highest level an exchange prize table is looked for at. The game's grade tables stop at 12.
    /// </summary>
    private const int MAX_EXCHANGE_LEVEL = 12;

    /// <summary>
    ///     The cosmetics every account may wear whether it owns them or not - the server's own
    ///     <c>
    ///         free_cx
    ///     </c>
    ///     (js/old_common_functions.js:153). They reach a character through its class rather than on their own, so
    ///     <see cref="EnrichClasses" /> is the only thing that reads them.
    /// </summary>
    private static readonly string[] FREE_COSMETICS =
    [
        "makeup105",
        "makeup117",
        "mmakeup00",
        "fmakeup01",
        "fmakeup02",
        "fmakeup03"
    ];

    private static readonly ILog Log = LogManager.GetLogger(typeof(GameData));

    /// <summary>
    ///     The game-data version the data members were last generated against. AL.MemberGenerator emits the stamp as generated
    ///     output (dataMembers/version.txt); paste it here when refreshing the datums.
    /// </summary>
    public const int KNOWN_VERSION = 8507;

    /// <summary>
    ///     What a monster with no entry in the dimensions table is squared off at before its size multiplier.
    /// </summary>
    private const float UNSIZED_HIT_BOX = 24f;

    /// <summary>
    ///     The hit box every player is measured against for range: 26 wide and 36 tall, fixed for everyone rather than read
    ///     from the dimensions table, which carries a different height for the same entry and is not what range is resolved
    ///     with. Their
    ///     <i>
    ///         collision
    ///     </i>
    ///     box is a separate and much smaller thing - the pathfinding default.
    /// </summary>
    public static readonly BoundingBase DEFAULT_CHARACTER_HIT_BOX = new(13f, 36f, 0f);

    [GameDataRoot]
    public static AchievementsDatum Achievements { get; private set; }

    [GameDataRoot]
    public static ClassesDatum Classes { get; private set; }

    [GameDataRoot]
    public static ConditionsDatum Conditions { get; private set; }

    //defaulted for the reason Multipliers is: a payload missing "cosmetics" degrades to empty tables rather than
    //throwing, and a character nothing can be dressed in is a better failure than a load that never finishes
    [GameDataRoot]
    public static GCosmetics Cosmetics { get; private set; } = new();

    [GameDataRoot]
    public static CraftDatum Craft { get; private set; }

    [GameDataRoot]
    public static DimensionsDatum Dimensions { get; private set; }

    [GameDataRoot]
    public static DismantleDatum Dismantle { get; private set; }

    [GameDataRoot]
    public static EventsDatum Events { get; private set; }

    [GameDataRoot]
    public static GamesDatum Games { get; private set; }

    [GameDataRoot]
    public static GeometryDatum Geometry { get; private set; }

    [GameDataRoot]
    public static IReadOnlyDictionary<string, GImageSet> ImageSets { get; private set; } = new Dictionary<string, GImageSet>();

    //the three art roots below plus Positions are defaulted for the reason Multipliers is: a payload missing any of
    //them degrades to an empty table rather than throwing, and nothing that reads them can do more than draw nothing
    [GameDataRoot]
    public static IReadOnlyDictionary<string, GImage> Images { get; private set; } = new Dictionary<string, GImage>();

    [GameDataRoot]
    public static ItemsDatum Items { get; private set; }

    [GameDataRoot]
    public static IReadOnlyDictionary<int, float> Levels { get; private set; } = new Dictionary<int, float>();

    [GameDataRoot]
    public static MapsDatum Maps { get; private set; }

    [GameDataRoot]
    public static MonstersDatum Monsters { get; private set; }

    [GameDataRoot]
    public static NPCsDatum NPCs { get; private set; }

    /// <summary>
    ///     Where every named piece of item art sits on its sheet, keyed by the skin an item names.
    /// </summary>
    [GameDataRoot]
    public static IReadOnlyDictionary<string, GSpritePosition> Positions { get; private set; } = new Dictionary<string, GSpritePosition>();

    [GameDataRoot]
    public static ProjectilesDatum Projectiles { get; private set; }

    [JsonIgnore]
    public static IReadOnlyDictionary<Quest, GNPC> Quests { get; private set; }

    [GameDataRoot]
    public static SetsDatum Sets { get; private set; }

    [GameDataRoot]
    public static SkillsDatum Skills { get; private set; }

    /// <summary>
    ///     The character and monster sheets, keyed by sheet name rather than by the skins they hold.
    /// </summary>
    [GameDataRoot]
    public static IReadOnlyDictionary<string, GSprite> Sprites { get; private set; } = new Dictionary<string, GSprite>();

    [GameDataRoot]
    public static TitlesDatum Titles { get; private set; }

    [GameDataRoot]
    public static TokensDatum Tokens { get; private set; }

    [GameDataRoot]
    public static int Version { get; private set; }

    //defaulted for the same reason Multipliers is: a payload missing "drops" degrades to an empty table rather
    //than throwing, and every consumer already has to handle a monster that drops nothing
    [GameDataRoot]
    public static GDrops Drops
    {
        get;

        [UsedImplicitly]
        private set;
    } = new();

    //defaulted so a payload missing "multipliers" degrades to zeroed ratios instead of throwing. The setter is
    //what Bind needs to reach it at all - get-only, it was skipped by the setter filter and every ratio stayed 0
    [GameDataRoot]
    public static GMultipliers Multipliers
    {
        get;

        [UsedImplicitly]
        private set;
    } = new();

    [JsonIgnore]
    public static int ShellsToGold => Multipliers.ShellsToGold;

    private static void AddBorderWalls()
    {
        foreach (var mapGeometry in Geometry.Values.DistinctBy(mapGeometry => mapGeometry.Accessor))
        {
            var top = new StraightLine(
                Convert.ToInt32(mapGeometry.Top),
                Convert.ToInt32(mapGeometry.Left),
                Convert.ToInt32(mapGeometry.Right),
                false);

            var right = new StraightLine(
                Convert.ToInt32(mapGeometry.Right),
                Convert.ToInt32(mapGeometry.Top),
                Convert.ToInt32(mapGeometry.Bottom),
                true);

            var bottom = new StraightLine(
                Convert.ToInt32(mapGeometry.Bottom),
                Convert.ToInt32(mapGeometry.Left),
                Convert.ToInt32(mapGeometry.Right),
                false);

            var left = new StraightLine(
                Convert.ToInt32(mapGeometry.Left),
                Convert.ToInt32(mapGeometry.Top),
                Convert.ToInt32(mapGeometry.Bottom),
                true);

            var horizontalLines = (List<StraightLine>)mapGeometry.HorizontalLines;
            var verticalLines = (List<StraightLine>)mapGeometry.VerticalLines;

            horizontalLines.Add(top);
            horizontalLines.Add(bottom);
            verticalLines.Add(left);
            verticalLines.Add(right);
        }
    }

    //System.Text.Json cannot bind static members, so drive the G-data statics from the wire by reflection:
    //for each [JsonProperty] static, deserialize the matching (case-insensitively, as Newtonsoft matched) wire
    //key through the shared options. An absent key leaves the member's initializer (Levels/Multipliers) intact.
    private static JsonObject Bind(string json)
    {
        var root = JsonNode.Parse(json)
                           ?.AsObject()
                   ?? throw new InvalidOperationException("Game data is not a JSON object.");

        var members = typeof(GameData).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                      .Where(property => property.GetCustomAttribute<GameDataRootAttribute>() is not null);

        foreach (var member in members)
        {
            //this used to be part of the filter above, which silently skipped a get-only member and left it on
            //its initializer forever - Multipliers read 0 for every ratio that way. A missing setter is a
            //declaration error, so say so at init rather than serving zeroes for the process's lifetime
            if (member.GetSetMethod(true) is null)
                throw new InvalidOperationException($"[GameDataRoot] {member.Name} has no setter, so it can never bind.");

            var wireName = member.GetCustomAttribute<JsonPropertyNameAttribute>()
                                 ?.Name
                           ?? member.Name;

            var node = root.FirstOrDefault(pair => string.Equals(pair.Key, wireName, StringComparison.OrdinalIgnoreCase))
                           .Value;

            if (node is not null)
                member.SetValue(null, node.Deserialize(member.PropertyType, ALJson.Options));
        }

        return root;
    }

    public static void BuildBoundingBases()
    {
        Log.Debug("Building monster bounding bases");

        foreach ((var accessor, var monster) in Monsters.Entries.DistinctBy(kvp => kvp.Value.Accessor))
        {
            var dimensions = Dimensions[accessor] ?? Array.Empty<float>();
            float h;
            float v;
            const float VN = 2;

            if ((dimensions.Count > 0) && (dimensions.ElementAtOrDefault(3) != 0))
            {
                h = dimensions.ElementAtOrDefault(3);

                //v + vn has to stay under 12
                v = Math.Min(9.9f, dimensions.ElementAtOrDefault(4));
            } else
            {
                h = Math.Min(12f, dimensions.ElementAtOrDefault(0) * 0.8f);

                if (h == 0)
                {
                    h = 8;
                    v = 7;
                } else
                    v = Math.Min(9.9f, dimensions.ElementAtOrDefault(1) / 4f);
            }

            //this is the collision box the game walks and pathfinds with, and is deliberately not the hit box below -
            //range is resolved against the whole sprite, but movement against a small foot-print at its base
            monster.BoundingBase = new BoundingBase(h, v, VN);

            //the hit box every range check is resolved against, which is the sprite rather than the foot-print above:
            //centred horizontally and rising from the monster's feet. A monster with no entry is squared off at 24,
            //and the handful carrying a size multiplier are scaled first - a crab is half size
            var hitWidth = dimensions.Count > 0 ? dimensions.ElementAtOrDefault(0) : UNSIZED_HIT_BOX;
            var hitHeight = dimensions.Count > 0 ? dimensions.ElementAtOrDefault(1) : UNSIZED_HIT_BOX;

            if (monster.Size != 0f)
            {
                hitWidth = MathF.Round(hitWidth * monster.Size);
                hitHeight = MathF.Round(hitHeight * monster.Size);
            }

            monster.HitBox = new BoundingBase(hitWidth / 2f, hitHeight, 0f);
        }
    }

    //removes wall geometry inside the rect and walls off its long sides, leaving a walkable vertical corridor
    //connecting whatever the rect's two short ends overlap
    private static void CarveCorridor(
        string mapAccessor,
        int left,
        int right,
        int top,
        int bottom)
    {
        var geometry = Geometry[mapAccessor];

        if (geometry == null)
        {
            Log.Warn($"No geometry for {mapAccessor}, corridor not carved");

            return;
        }

        var verticalLines = ClipLines(
                geometry.VerticalLines,
                left,
                right,
                top,
                bottom)
            .ToList();

        //seal the long sides so the carve connects only the two mouths, not the water beyond them
        verticalLines.Add(
            new StraightLine(
                left,
                top,
                bottom,
                true));

        verticalLines.Add(
            new StraightLine(
                right,
                top,
                bottom,
                true));

        geometry.VerticalLines = verticalLines;

        geometry.HorizontalLines = ClipLines(
                geometry.HorizontalLines,
                top,
                bottom,
                left,
                right)
            .ToList();
    }

    //corridors that exist only in local data. the server never traces the segment between a move's endpoints -
    //it checks the endpoints against its walkable lattice (jail) and prices the cells crossed (movement penalty) -
    //so a carved channel lets the pathfinder route a crossing the game's own geometry forbids
    private static void CarveCorridors()

        //winterland ice golem island: the island is legal ground to the server (spawns 6 and 7 sit on it), and this
        //is the lake's narrowest water - 64 units where every other column is 80 or more. Both mouths round onto
        //lattice cells the server accepts, and 22 wide leaves a 6px channel after the wall padding
        => CarveCorridor(
            "winterland",
            733,
            755,
            272,
            352);

    //drops the portion of each line inside the window: a line strictly between the on-axis bounds is clipped
    //to the span bounds, splitting into up to two pieces. lines on the window edge merge with the seals instead
    private static IEnumerable<StraightLine> ClipLines(
        IEnumerable<StraightLine> lines,
        int onMin,
        int onMax,
        int spanMin,
        int spanMax)
    {
        foreach (var line in lines)
        {
            if ((line.On <= onMin) || (line.On >= onMax))
            {
                yield return line;

                continue;
            }

            var start = Math.Min(line.Start, line.End);
            var end = Math.Max(line.Start, line.End);

            if ((end <= spanMin) || (start >= spanMax))
            {
                yield return line;

                continue;
            }

            if (start < spanMin)
                yield return new StraightLine(
                    line.On,
                    start,
                    spanMin,
                    line.IsVertical);

            if (end > spanMax)
                yield return new StraightLine(
                    line.On,
                    spanMax,
                    end,
                    line.IsVertical);
        }
    }

    /// <summary>
    ///     Counts wire members across the datum-backed roots that no generated property declares - the signal that
    ///     AL.MemberGenerator actually needs a re-run, as opposed to a version bump that only changed values.
    /// </summary>
    internal static int CountUnknownMembers(JsonObject root)
    {
        var count = 0;

        foreach (var member in typeof(GameData).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (member.GetCustomAttribute<GameDataRootAttribute>() is null)
                continue;

            if (!IsDatum(member.PropertyType))
                continue;

            var wireName = member.GetCustomAttribute<JsonPropertyNameAttribute>()
                                 ?.Name
                           ?? member.Name;

            var node = root.FirstOrDefault(pair => string.Equals(pair.Key, wireName, StringComparison.OrdinalIgnoreCase))
                           .Value;

            if (node is not JsonObject section)
                continue;

            //the same property filter BuildLookupTable applies: readable, not an indexer, not enrichment-only
            var declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in member.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead
                    || (property.GetIndexParameters()
                                .Length
                        != 0)
                    || property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                    continue;

                declared.Add(
                    property.GetCustomAttribute<JsonPropertyNameAttribute>()
                            ?.Name
                    ?? property.Name);
            }

            count += section.Count(pair => !declared.Contains(pair.Key));
        }

        return count;

        static bool IsDatum(Type type)
        {
            for (var current = type; current is not null; current = current.BaseType)
                if (current.IsGenericType && (current.GetGenericTypeDefinition() == typeof(DatumBase<>)))
                    return true;

            return false;
        }
    }

    /// <summary>
    ///     Where the server lets a door open from, as a band plus a range. The server measures a door-sized box standing on
    ///     the door's own spawn against the character's 26 by 36 box, per axis, clamped at zero, and opens the door under 112.
    ///     The character positions whose box touches the door's box form a rectangle (the door box grown by the character
    ///     box), and the region is that rectangle inflated by the range.
    /// </summary>
    /// <returns>
    ///     The band and range, or a zero-size band on the door with no range for a door whose spawn cannot be resolved - the
    ///     server faults on the same missing spawn, and a range there would stop the walk somewhere the door does not open.
    /// </returns>
    private static (Rectangle Band, float Range) DoorReachBand(GMap map, GDoor door)
    {
        var spawnId = (int)door.CurrentMapSpawnId;

        if ((spawnId < 0) || (spawnId >= map.Spawns.Count))
            return (new Rectangle(
                door.X,
                door.Y,
                0f,
                0f), 0f);

        var spawn = map.Spawns[spawnId];
        var halfWidth = door.Width / 2 + CONSTANTS.CHARACTER_BOX_WIDTH / 2;
        var top = spawn.Y - door.Height;
        var bottom = spawn.Y + CONSTANTS.CHARACTER_BOX_HEIGHT;

        var band = new Rectangle(new Point(spawn.X - halfWidth, top), new Point(spawn.X + halfWidth, bottom));

        //a door sitting outside its own region means the spawn is not the one the server pairs with it, so the
        //band is not to be trusted either - walk to the door itself rather than to a region we just disproved
        if (band.EdgeToCenterDistance(door) >= CONSTANTS.DOOR_RANGE)
        {
            Log.Warn($"Door {map.Accessor} => {door.DestinationMap} lies outside the range of spawn {spawnId}.");

            return (new Rectangle(
                door.X,
                door.Y,
                0f,
                0f), 0f);
        }

        return (band, CONSTANTS.DOOR_RANGE);
    }

    /// <summary>
    ///     Finishes every class's exclusive-cosmetic list the way the server's own game-data pass does
    ///     (js/old_common_functions.js:171-182): the free makeups, then the name and every per-slot piece of each of the
    ///     class's <see cref="GClassLook" />s.
    /// </summary>
    /// <remarks>
    ///     The payload is the raw list, not the finished one - most classes send nothing for it at all, and none of them names
    ///     its own looks. A character is entitled to those, so without this pass anything asking what a class may wear is
    ///     missing its default looks and answers
    ///     <c>
    ///         cx_not_found
    ///     </c>
    ///     on a name the server would have taken. Each push is guarded the way the server guards it, so a name already granted
    ///     outright is not repeated.
    /// </remarks>
    private static void EnrichClasses()
    {
        Log.Debug("Enriching class cosmetics");

        foreach (var gClass in Classes.Values)
        {
            var exclusives = new List<string>(gClass.ExclusiveCosmetics);

            foreach (var cosmetic in FREE_COSMETICS)
                if (!exclusives.Contains(cosmetic))
                    exclusives.Add(cosmetic);

            //a look short of its name or its slot map is skipped rather than thrown on. The server's own loop
            //shrugs the same gap off, and this runs inside Populate - throwing here would stop every character
            //logging in over one malformed entry
            foreach (var look in gClass.Looks)
            {
                if (!exclusives.Contains(look.Name))
                    exclusives.Add(look.Name);

                foreach (var piece in look.Pieces.Values)
                    if (!exclusives.Contains(piece))
                        exclusives.Add(piece);
            }

            gClass.ExclusiveCosmetics = exclusives;
        }
    }

    private static void EnrichDrops()
    {
        Log.Debug("Enriching drop tables");

        if (Drops.Unbound is not { Count: > 0 })
            return;

        var tables = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

        foreach ((var dropId, var element) in Drops.Unbound)
        {
            //defensive about shape rather than about presence: every leftover key in the committed data is an array
            //of drop entries, and a future scalar would otherwise throw out of startup
            if (element.ValueKind != JsonValueKind.Array)
                continue;

            if (element.Deserialize<IReadOnlyList<GDrop>>() is { } table)
                tables[dropId] = table;
        }

        Drops.Tables = tables;
    }

    private static void EnrichItems()
    {
        Log.Debug("Enriching item metadata");

        //--CONNECT ITEM DATA--
        //connect item recipes
        foreach ((var itemName, var recipe) in Craft.Entries)
        {
            var item = Items[itemName];

            if (item != null)
                item.Recipe = recipe;
        }

        //connect item ObtainableFromNPC. Placed sellers first: this is a first-writer race and CanBuy ends on
        //ObtainableFromNPC.Locations.Any, so an item resolved to a seller standing only on ignored maps is unbuyable
        //with nothing logged anywhere. OrderByDescending is stable, so the datum's own order still decides among peers
        foreach (var npc in NPCs.Values
                                .DistinctBy(npc => npc.Id)
                                .OrderByDescending(npc => npc.Locations.Count > 0))
            if (npc.Items != null)
                foreach (var itemName in npc.Items)
                {
                    if (itemName == null)
                        continue;

                    var item = Items[itemName];

                    if (item is { ObtainableFromNPC: null })
                    {
                        item.ObtainableFromNPC = npc;
                        item.ObtainType = ObtainType.Buy;
                    }
                }

        foreach (var item in Items.Values.DistinctBy(item => item.Accessor))
        {
            if (item.ObtainableFromNPC == null)
                if (!string.IsNullOrEmpty(item.NPC))
                {
                    var npc = NPCs[item.NPC];

                    if (npc != null)
                    {
                        item.ObtainableFromNPC = NPCs[item.NPC];

                        //(monstertoken)
                        item.ObtainType = ObtainType.Quest;
                    }
                } else if (item.Recipe?.NPC != null)
                {
                    item.ObtainableFromNPC = item.Recipe.NPC;
                    item.ObtainType = ObtainType.Craft;
                }

            //exchange at, as the server's own rule: the item's quest npc when it carries a quest tag, and the one
            //fixed exchange placement otherwise (node/server.js:6073). Gated on exchangeability, since that is what
            //the field means. GetValueOrDefault rather than the indexer, or a missing quest throws out of startup
            if (item.ExchangeCount.HasValue)
            {
                item.ExchangeAtNPC = item.Quest is { } quest ? Quests.GetValueOrDefault(quest) : NPCs[EXCHANGE_NPC];

                //the prizes, keyed the way the server keys the table it rolls: the item's name plus its level when
                //the item compounds or upgrades, and the bare name otherwise (node/server.js:6067-6068). Assembled
                //here and nowhere else - a drop id built a second time is one that drifts
                item.ExchangeRewards = ExchangeRewardsFor(item);
            }
        }

        foreach ((var tokenName, var buyableItems) in Tokens.Entries)
            foreach (var itemName in buyableItems.Keys)
            {
                var item = Items[itemName];

                if (item is { ObtainableFromNPC: null })
                    foreach (var npc in NPCs.Values.DistinctBy(npc => npc.Id))
                        if (npc.Token
                               .ToString()
                               .EqualsI(tokenName))
                        {
                            item.ObtainableFromNPC = npc;
                            item.ObtainType = ObtainType.Exchange;

                            break;
                        }
            }
    }

    private static void EnrichMaps()
    {
        Log.Debug("Enriching map metadata");

        //--CONNECT MAP DATA--
        foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
        {
            if (map.Ignore)
                continue;

            //empty rather than absent for a map with no table, so nothing downstream distinguishes two kinds of nothing
            map.Drops = Drops.Maps.GetValueOrDefault(map.Accessor) ?? [];

            var geometry = Geometry[map.Accessor];
            var exits = (List<Exit>)map.Exits;

            //connect npc data
            foreach (var npc in map.NPCs)
            {
                var nData = NPCs[npc.Id];
                npc.Data = NPCs[npc.Id]!;

                if (nData == null)
                {
                    Log.Warn($"NPC {npc.Id} is missing metadata.");

                    continue;
                }

                //locations for this map
                var locations = (List<Location>)npc.Locations;

                if (npc._position != null)
                {
                    var position = npc._position;

                    locations.Add(new Location(map.Accessor, position));
                }

                if (npc._positions != null)
                    foreach (var position in npc._positions)
                        locations.Add(new Location(map.Accessor, position));

                //populate exits with transport npc data
                if (nData is { Role: NPCRole.Transport, Places: not null })
                    foreach ((var mapAccessor, var spawnId) in nData.Places)
                        foreach (var location in locations)
                        {
                            var toMapData = Maps[mapAccessor];

                            if ((toMapData == null) || toMapData.Accessor.EqualsI(map.Accessor))
                                continue;

                            var spawn = toMapData.Spawns[spawnId];

                            exits.Add(
                                new Exit(
                                    map.Accessor,
                                    location,
                                    new Location(mapAccessor, spawn),
                                    spawnId,
                                    ExitType.Transporter,
                                    new Rectangle(
                                        location.X,
                                        location.Y,
                                        0f,
                                        0f),
                                    CONSTANTS.TRANSPORTER_RANGE));
                        }
            }

            //connect monster data
            foreach (var monster in map.Monsters)
            {
                monster.Data = Monsters[monster.Name]!;
                var boundaries = (List<InscribedBoundary>)monster.Boundaries;

                //boundaries for this map
                if (monster._boundary != null)
                {
                    var boundary = monster._boundary;
                    var boundaryMap = boundary.Map == string.Empty ? map.Accessor : boundary.Map;

                    boundaries.Add(new InscribedBoundary(boundary, boundaryMap));
                }

                if (monster._boundaries != null)
                {
                    var mBoundaries = monster._boundaries;

                    foreach (var boundary in mBoundaries)
                    {
                        var boundaryMap = boundary.Map == string.Empty ? map.Accessor : boundary.Map;

                        boundaries.Add(new InscribedBoundary(boundary, boundaryMap));
                    }
                }
            }

            //connect map to it's geometry
            if (geometry != null)
                map.Geomertry = geometry;

            //populate exits with door data
            foreach (var door in map.Doors)
            {
                var toMapData = Maps[door.DestinationMap];

                if (toMapData == null)
                    continue;

                var spawn = toMapData.Spawns[door.DestinationSpawnId];
                (var band, var range) = DoorReachBand(map, door);

                exits.Add(
                    new Exit(
                        map.Accessor,
                        door,
                        new Location(door.DestinationMap, spawn),
                        door.DestinationSpawnId,
                        ExitType.Door,
                        band,
                        range));
            }
        }
    }

    private static void EnrichMonsters()
    {
        Log.Debug("Enriching monster metadata");

        foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
        {
            if (map.Ignore)
                continue;

            foreach (var monster in map.Monsters)
            {
                var mData = monster.Data;

                if (mData == null)
                {
                    Log.Warn($"Monster {monster.Name} is missing metadata.");

                    continue;
                }

                var spawnAreas = (List<InscribedBoundary>)mData.SpawnAreas;
                spawnAreas.AddRange(monster.Boundaries);
            }
        }
    }

    private static void EnrichNPCs()
    {
        Log.Debug("Enriching npc metadata");

        foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
        {
            if (map.Ignore)
                continue;

            foreach (var npc in map.NPCs)
            {
                var nData = npc.Data;

                if (nData == null)
                {
                    Log.Warn($"NPC {npc.Id} is missing metadata.");

                    continue;
                }

                var locations = (List<Location>)nData.Locations;
                locations.AddRange(npc.Locations);
            }
        }
    }

    private static void EnrichQuests()
    {
        Log.Debug("Enriching quest metadata");

        var quests = new Dictionary<Quest, GNPC>();

        foreach (var npc in NPCs.Values.DistinctBy(npc => npc.Id))
            if (npc.Quest != Quest.None)
                quests[npc.Quest] = npc;

        Quests = quests;
    }

    private static void EnrichRecipes()
    {
        Log.Debug("Enriching recipe metadata");
        var craftsman = NPCs["craftsman"]!;

        //--CONNECT RECIPE DATA--
        foreach (var recipe in Craft.Values)
            if (recipe.Quest.HasValue && (recipe.Quest.Value != Quest.None))
                recipe.NPC = Quests[recipe.Quest.Value];
            else
                recipe.NPC = craftsman;

        //no dismantle recipe carries a quest tag today, but the fallback matters: the server requires the craftsman
        //for every dismantle (node/server.js:5892), and NPC is non-nullable
        foreach (var recipe in Dismantle.Values)
            if (recipe.Quest.HasValue && (recipe.Quest.Value != Quest.None))
                recipe.NPC = Quests[recipe.Quest.Value];
            else
                recipe.NPC = craftsman;
    }

    /// <summary>
    ///     Folds each set tier into the next, so a tier carries what the server applies at that count rather than only its own
    ///     line.
    /// </summary>
    /// <remarks>
    ///     The server does this to its own copy of G at boot -
    ///     <c>
    ///         sprocess_game_data
    ///     </c>
    ///     ,
    ///     <c>
    ///         node/server_functions.js:248-260
    ///     </c>
    ///     - and then applies the single entry matching the worn count,
    ///     <c>
    ///         node/server.js:1255-1261
    ///     </c>
    ///     . That rollup never reaches the appengine copy this downloads, so what arrives here is per-tier deltas. Reading a
    ///     delta as the count's value understates it badly: heavy armor at five pieces lists 16 fortitude and grants 38.
    ///     <br />
    ///     The server's loop stops at
    ///     <c>
    ///         items.length
    ///     </c>
    ///     , so a count past the top authored tier keeps the top total - twelve vampire pieces is what three is. Reproduced
    ///     here by running the ladder to the member count rather than to the highest tier the wire authored.
    /// </remarks>
    private static void EnrichSets()
    {
        foreach (var set in Sets.Values)
        {
            //the wire tiers are cleared once folded, so a set reached twice - the lookup can file one object under
            //more than one key - is skipped rather than having its ladder rebuilt from nothing
            if (set.WireTiers is null)
                continue;

            var tiers = new List<GSetTier>(set.Items.Count);
            var running = new Dictionary<ALAttribute, float>();

            for (var pieces = 1; pieces <= set.Items.Count; pieces++)
            {
                var adds = set.WireTiers.TryGetValue(pieces.ToString(CultureInfo.InvariantCulture), out var element)
                    ? element.Deserialize<GSetBonus>(ALJson.Options) ?? new GSetBonus()
                    : new GSetBonus();

                foreach ((var attribute, var amount) in adds.Attributes)
                    running[attribute] = running.GetValueOrDefault(attribute) + amount;

                tiers.Add(
                    new GSetTier
                    {
                        Pieces = pieces,
                        Adds = adds,

                        //copied rather than shared: the running total keeps being added to, and every tier would
                        //otherwise end up holding the last one
                        InEffect = new GSetBonus
                        {
                            Attributes = new Dictionary<ALAttribute, float>(running)
                        }
                    });
            }

            set.Tiers = tiers;
            set.WireTiers = null;
        }
    }

    /// <summary>
    ///     Every prize table this item can be exchanged for, by level, or
    ///     <c>
    ///         null
    ///     </c>
    ///     where the data has none.
    /// </summary>
    private static IReadOnlyDictionary<int, IReadOnlyList<GDrop>>? ExchangeRewardsFor(GItem item)
    {
        //compound and upgrade arrive as the stat tables they scale, so "either is present" is the server's own test
        if (item is { CompoundModifiers: null, UpgradeModifiers: null })
            return Drops.Tables.GetValueOrDefault(item.Accessor) is { } table
                ? new Dictionary<int, IReadOnlyList<GDrop>>
                {
                    [0] = table
                }
                : null;

        var levelled = new Dictionary<int, IReadOnlyList<GDrop>>();

        //asked level by level rather than scanned, because the id is a string the game writes per level. The bound is
        //the highest level any grade table reaches; a level past the last table has no prize and is not exchangeable
        for (var level = 0; level <= MAX_EXCHANGE_LEVEL; level++)
            if (Drops.Tables.GetValueOrDefault(item.Accessor + level) is { } table)
                levelled[level] = table;

        return levelled.Count > 0 ? levelled : null;
    }

    private static void FixLines()
    {
        Log.Debug("Merging overlapped lines");

        foreach (var mapGeometry in Geometry.Values.DistinctBy(mapGeometry => mapGeometry.Accessor))
        {
            mapGeometry.VerticalLines = LineHelper.FixLines(mapGeometry.VerticalLines, true);
            mapGeometry.HorizontalLines = LineHelper.FixLines(mapGeometry.HorizontalLines, false);
        }
    }

    public static void Populate(string json)
    {
        var stopwatch = Stopwatch.StartNew();

        Log.Info("Deserializing game data");
        var root = Bind(json);

        //a version bump alone is not actionable - the data members only need regenerating when the live data
        //carries members they do not declare
        if (Version > KNOWN_VERSION)
        {
            var unknownMembers = CountUnknownMembers(root);

            if (unknownMembers > 0)
                Log.Warn(
                    $"Server game data is version {Version}, newer than the version the data members were generated against ({KNOWN_VERSION}),"
                    + $" and carries {unknownMembers} members they do not declare. Re-run AL.MemberGenerator.");
        }

        Log.Info("Constructing data lookups");
        Achievements.BuildLookupTable();
        Classes.BuildLookupTable();
        Conditions.BuildLookupTable();
        Craft.BuildLookupTable();
        Dimensions.BuildLookupTable();
        Dismantle.BuildLookupTable();
        Events.BuildLookupTable();
        Geometry.BuildLookupTable();
        Items.BuildLookupTable();
        Maps.BuildLookupTable();
        Monsters.BuildLookupTable();
        NPCs.BuildLookupTable();
        Projectiles.BuildLookupTable();
        Sets.BuildLookupTable();
        Skills.BuildLookupTable();
        Titles.BuildLookupTable();
        Tokens.BuildLookupTable();

        //fix line data (merge lines, set isX for x lines)
        AddBorderWalls();
        FixLines();

        //local-only geometry edits: open walkable corridors through walls the server tolerates crossing
        CarveCorridors();

        Log.Info("Enriching data");

        //populate quest dictionary with npcs
        EnrichQuests();

        //drops first: the map and item passes below both hang tables off what this builds, and a pass reading an
        //empty Tables would enrich nothing and say nothing
        EnrichDrops();

        //connect various data points. NPCs before items, because the item pass now prefers a seller that is actually
        //placed and GNPC.Locations is empty until EnrichNPCs fills it - EnrichMaps has already put the per-map entries
        //and npc.Data in place, which is all EnrichNPCs itself needs
        EnrichRecipes();
        EnrichMaps();
        EnrichNPCs();
        EnrichItems();
        EnrichSets();
        EnrichMonsters();
        EnrichClasses();
        BuildBoundingBases();

        stopwatch.Stop();
        Log.Info($"Serialized data in {stopwatch.ElapsedMilliseconds}ms");
    }
}