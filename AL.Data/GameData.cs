#region
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.Core.Interfaces;
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
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Monsters;
using AL.Data.Multipliers;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.Data.Tokens;
using Chaos.Extensions.Common;
using Common.Logging;
#endregion

//the G-data statics are written only by Bind's reflection, which requires GetSetMethod(true) to be non-null
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AL.Data;

public record GameData
{
    /// <summary>
    ///     The npc every exchange with no quest tag of its own is measured against - <c>G.maps.main.exchange</c>,
    ///     which the map data carries as a placement of this id on <c>main</c> alone. The copy on
    ///     <c>original_main</c> sits on a map marked ignored, so <see cref="EnrichNPCs" /> never adds it to
    ///     <c>Locations</c> - the same skip the server's own placement loop takes
    ///     (<c>js/old_common_functions.js:197</c>, filling <c>map.exchange</c> at <c>:236</c>).
    /// </summary>
    private const string EXCHANGE_NPC = "exchange";

    private static readonly ILog Log = LogManager.GetLogger(typeof(GameData));

    [GameDataRoot]
    public static AchievementsDatum Achievements { get; private set; }

    [GameDataRoot]
    public static ClassesDatum Classes { get; private set; }

    [GameDataRoot]
    public static ConditionsDatum Conditions { get; private set; }

    [GameDataRoot]
    public static CraftDatum Craft { get; private set; }

    [GameDataRoot]
    public static DimensionsDatum Dimensions { get; private set; }

    [GameDataRoot]
    public static DismantleDatum Dismantle { get; private set; }

    //defaulted for the same reason Multipliers is: a payload missing "drops" degrades to an empty table rather
    //than throwing, and every consumer already has to handle a monster that drops nothing
    [GameDataRoot]
    public static GDrops Drops { get; private set; } = new();

    [GameDataRoot]
    public static EventsDatum Events { get; private set; }

    [GameDataRoot]
    public static GamesDatum Games { get; private set; }

    [GameDataRoot]
    public static GeometryDatum Geometry { get; private set; }

    [GameDataRoot]
    public static ItemsDatum Items { get; private set; }

    [GameDataRoot]
    public static IReadOnlyDictionary<int, float> Levels { get; private set; } = new Dictionary<int, float>();

    [GameDataRoot]
    public static MapsDatum Maps { get; private set; }

    [GameDataRoot]
    public static MonstersDatum Monsters { get; private set; }

    //defaulted so a payload missing "multipliers" degrades to zeroed ratios instead of throwing. The setter is
    //what Bind needs to reach it at all - get-only, it was skipped by the setter filter and every ratio stayed 0
    [GameDataRoot]
    public static GMultipliers Multipliers { get; private set; } = new();

    [GameDataRoot]
    public static NPCsDatum NPCs { get; private set; }

    [GameDataRoot]
    public static ProjectilesDatum Projectiles { get; private set; }

    [JsonIgnore]
    public static IReadOnlyDictionary<Quest, GNPC> Quests { get; private set; }

    [GameDataRoot]
    public static SkillsDatum Skills { get; private set; }

    [GameDataRoot]
    public static TitlesDatum Titles { get; private set; }

    [GameDataRoot]
    public static TokensDatum Tokens { get; private set; }

    [GameDataRoot]
    public static int Version { get; private set; }

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
    private static void Bind(string json)
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
    }

    /// <summary>
    ///     What a monster with no entry in the dimensions table is squared off at before its size multiplier.
    /// </summary>
    private const float UNSIZED_HIT_BOX = 24f;

    /// <summary>
    ///     The hit box every player is measured against for range: 26 wide and 36 tall, fixed for everyone rather than
    ///     read from the dimensions table, which carries a different height for the same entry and is not what range
    ///     is resolved with. Their <i>collision</i> box is a separate and much smaller thing - the pathfinding default.
    /// </summary>
    public static readonly BoundingBase DEFAULT_CHARACTER_HIT_BOX = new(13f, 36f, 0f);

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
            //centred horizontally and rising from the monster's feet, so the whole height sits on one side of it. A
            //monster the table has no entry for is squared off at 24 rather than left without a box, and the handful
            //carrying a size multiplier are scaled and rounded before anything measures them - a crab is half size,
            //so getting this wrong is worth several units on the most common target there is
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
            //fixed exchange placement otherwise (node/server.js:6073). Stated once rather than filled in from an
            //npc's token, which named the wrong npc for the four tokens and left the field null for every other
            //exchangeable - of the 38 items carrying an exchange count in the committed game data, 31 resolve to the
            //fixed placement and 7 to a
            //quest npc. Gated on exchangeability because that is what this field means - "if
            //populated, this item can be exchanged at this npc" - and 2 of the 9 items carrying a quest tag are not
            //exchangeable at all. GetValueOrDefault rather than the indexer: this runs at data load, where a quest
            //the npc table has no entry for would throw out of startup instead of leaving one item unresolved
            if (item.ExchangeCount.HasValue)
                item.ExchangeAtNPC = item.Quest is { } quest ? Quests.GetValueOrDefault(quest) : NPCs[EXCHANGE_NPC];
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
                if ((nData.Role == NPCRole.Transport) && (nData.Places != null))
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
                (var radius, var reachableFrom) = DoorReachableRegion(map, door);

                exits.Add(
                    new Exit(
                        map.Accessor,
                        door,
                        new Location(door.DestinationMap, spawn),
                        door.DestinationSpawnId,
                        ExitType.Door,
                        radius,
                        reachableFrom));
            }
        }
    }

    /// <summary>
    ///     Builds the region a door lets you through from, plus a single conservative circle about the door itself.
    /// </summary>
    /// <remarks>
    ///     The server does not measure a door from the door. It measures a box the size of the door, placed on the
    ///     spawn <see cref="GDoor.CurrentMapSpawnId" /> names, against the character's own box, taking the separation
    ///     on each axis and clamping it at zero. Both boxes hang upward from their positions, so the band is not
    ///     symmetric about the spawn. That makes the region a rectangle inflated by <c>DOOR_RANGE</c>. Circles on
    ///     the rectangle's four corners plus one on the door itself cover about 95% of it, never below 91% on the
    ///     current door table - each one wholly inside, so no circle here can claim reach the server would refuse.
    ///     <br />
    ///     A nought radius means the region could not be derived. It is not "no reach" so much as "walk all the way
    ///     to the door", which is what a nought turns off the shortcut into.
    /// </remarks>
    private static (float Radius, IReadOnlyList<ICircle>? ReachableFrom) DoorReachableRegion(GMap map, GDoor door)
    {
        var spawnId = (int)door.CurrentMapSpawnId;

        //a door whose entry names no spawn on this map is one the server cannot resolve either. the old behaviour
        //cannot stand in for it - that was a circle on the door, which is the model this replaced, and at this
        //range it would stop the walk somewhere the door does not open
        //
        //an absent id reads as spawn 0 rather than as absent, so this does not catch every one. it does not need
        //to: the server faults on the same missing spawn, so that door opens from nowhere and where we stand is
        //moot, and an id that is absent while spawn 0 sits far away still lands on the radius check below
        if ((spawnId < 0) || (spawnId >= map.Spawns.Count))
            return (0f, null);

        var spawn = map.Spawns[spawnId];
        var halfWidth = door.Width / 2 + CONSTANTS.CHARACTER_BOX_WIDTH / 2;
        var top = spawn.Y - door.Height;
        var bottom = spawn.Y + CONSTANTS.CHARACTER_BOX_HEIGHT;

        //the separation is a true distance to the rectangle, so shrinking the range by it leaves a circle about
        //the door that is still wholly inside the region
        var offsetX = MathF.Max(MathF.Abs(door.X - spawn.X) - halfWidth, 0f);
        var offsetY = MathF.Max(MathF.Max(door.Y - bottom, top - door.Y), 0f);
        var radius = CONSTANTS.DOOR_RANGE - MathF.Sqrt(offsetX * offsetX + offsetY * offsetY);

        //a door sitting outside its own region means the spawn is not the one the server pairs with it, so the
        //corners are not to be trusted either - walk to the door itself rather than to a region we just disproved
        if (radius <= 0)
        {
            Log.Warn($"Door {map.Accessor} => {door.DestinationMap} lies outside the range of spawn {spawnId}.");

            return (0f, null);
        }

        ICircle[] reachableFrom =
        [
            new Circle(spawn.X - halfWidth, top, CONSTANTS.DOOR_RANGE),
            new Circle(spawn.X + halfWidth, top, CONSTANTS.DOOR_RANGE),
            new Circle(spawn.X - halfWidth, bottom, CONSTANTS.DOOR_RANGE),
            new Circle(spawn.X + halfWidth, bottom, CONSTANTS.DOOR_RANGE),

            //corners alone leave the middle of a wide door uncovered - a 200 unit one puts its own position out of
            //reach of all four - so the door's circle rides along. it also keeps one candidate that always lies
            //between the character and the door, where a corner can sit off to the side
            new Circle(door, radius)
        ];

        return (radius, reachableFrom);
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
        Bind(json);

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
        Skills.BuildLookupTable();
        Titles.BuildLookupTable();
        Tokens.BuildLookupTable();

        //fix line data (merge lines, set isX for x lines)
        AddBorderWalls();
        FixLines();

        Log.Info("Enriching data");

        //populate quest dictionary with npcs
        EnrichQuests();

        //connect various data points. NPCs before items, because the item pass now prefers a seller that is actually
        //placed and GNPC.Locations is empty until EnrichNPCs fills it - EnrichMaps has already put the per-map entries
        //and npc.Data in place, which is all EnrichNPCs itself needs
        EnrichRecipes();
        EnrichMaps();
        EnrichNPCs();
        EnrichItems();
        EnrichMonsters();
        BuildBoundingBases();

        stopwatch.Stop();
        Log.Info($"Serialized data in {stopwatch.ElapsedMilliseconds}ms");
    }
}