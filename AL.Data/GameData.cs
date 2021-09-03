using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Helpers;
using AL.Data.Achievements;
using AL.Data.Classes;
using AL.Data.Conditions;
using AL.Data.Craft;
using AL.Data.Dimensions;
using AL.Data.Dismantle;
using AL.Data.Events;
using AL.Data.Games;
using AL.Data.Geometry;
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Monsters;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.Data.Tokens;
using Chaos.Core.Extensions;
using Common.Logging;
using Newtonsoft.Json;

#nullable enable
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AL.Data
{
    [JsonObject]
    public record GameData
    {
        private static ILog Log = LogManager.GetLogger(typeof(GameData));

        [JsonProperty]
        public static AchievementsDatum Achievements { get; private set; }

        [JsonProperty]
        public static ClassesDatum Classes { get; private set; }

        [JsonProperty]
        public static ConditionsDatum Conditions { get; private set; }

        [JsonProperty]
        public static CraftDatum Craft { get; private set; }

        [JsonProperty]
        public static DimensionsDatum Dimensions { get; private set; }

        [JsonProperty]
        public static DismantleDatum Dismantle { get; private set; }

        [JsonProperty]
        public static EventsDatum Events { get; private set; }

        [JsonProperty]
        public static GamesDatum Games { get; private set; }

        [JsonProperty]
        public static GeometryDatum Geometry { get; private set; }

        [JsonProperty]
        public static int Inflation { get; private set; }

        [JsonProperty]
        public static ItemsDatum Items { get; private set; }

        [JsonProperty]
        public static IReadOnlyDictionary<int, float> Levels { get; private set; } = new Dictionary<int, float>();

        [JsonProperty]
        public static MapsDatum Maps { get; private set; }

        [JsonProperty]
        public static MonstersDatum Monsters { get; private set; }

        [JsonProperty]
        public static NPCsDatum NPCs { get; private set; }

        [JsonProperty]
        public static ProjectilesDatum Projectiles { get; private set; }

        [JsonIgnore]
        public static IReadOnlyDictionary<Quest, GNPC> Quests { get; private set; }

        [JsonProperty("shells_to_gold")]
        public static int ShellsToGold { get; private set; }

        [JsonProperty]
        public static SkillsDatum Skills { get; private set; }

        [JsonProperty]
        public static TitlesDatum Titles { get; private set; }

        [JsonProperty]
        public static TokensDatum Tokens { get; private set; }

        //public Dimensions Dimensions { get; private set; }
        //public Sprites Sprites { get; private set; }
        //public Tilesets Tilesets { get; private set; }
        //public Sets Sets { get; private set; }
        //public Positions Positions { get; private set; }
        //public Images1 Images { get; private set; }
        //public Imagesets Imagesets { get; private set; }
        //public Docs Docs { get; private set; }
        //public Drops Drops { get; private set; }
        //public Emotions Emotions { get; private set; }
        //public Cosmetics Cosmetics { get; private set; }

        [JsonProperty]
        public static int Version { get; private set; }

        private static void AddBorderWalls()
        {
            foreach (var mapGeometry in Geometry.Values.DistinctBy(mapGeometry => mapGeometry.Accessor))
            {
                var top = new StraightLine(Convert.ToInt32(mapGeometry.Top), Convert.ToInt32(mapGeometry.Left),
                    Convert.ToInt32(mapGeometry.Right), false);

                var right = new StraightLine(Convert.ToInt32(mapGeometry.Right), Convert.ToInt32(mapGeometry.Top),
                    Convert.ToInt32(mapGeometry.Bottom), true);

                var bottom = new StraightLine(Convert.ToInt32(mapGeometry.Bottom), Convert.ToInt32(mapGeometry.Left),
                    Convert.ToInt32(mapGeometry.Right), false);

                var left = new StraightLine(Convert.ToInt32(mapGeometry.Left), Convert.ToInt32(mapGeometry.Top),
                    Convert.ToInt32(mapGeometry.Bottom), true);

                var horizontalLines = (List<StraightLine>)mapGeometry.HorizontalLines;
                var verticalLines = (List<StraightLine>)mapGeometry.VerticalLines;

                horizontalLines.Add(top);
                horizontalLines.Add(bottom);
                verticalLines.Add(left);
                verticalLines.Add(right);
            }
        }

        public static void BuildBoundingBases()
        {
            Log.Debug("Building monster bounding bases");

            foreach ((var accessor, var monster) in Monsters.DistinctBy(kvp => kvp.Value.Accessor))
            {
                var dimensions = Dimensions[accessor] ?? Array.Empty<float>();
                float h;
                float v;
                const float VN = 2;

                if ((dimensions.Count > 0) && (dimensions.ElementAtOrDefault(3) != 0))
                {
                    h = dimensions.ElementAtOrDefault(3);
                    v = Math.Min(9.9f, dimensions.ElementAtOrDefault(4));
                } else
                {
                    //TODO: Unsure if this is correct, the source's way of getting this data potentially includes UI data (get_width)
                    h = Math.Min(12f, dimensions.ElementAtOrDefault(0) * 0.8f);

                    if (h == 0)
                    {
                        h = 8;
                        v = 7;
                    } else
                        //TODO: Unsure if this is correct, the source's way of getting this data potentially includes UI data (get_height)
                        v = Math.Min(9.9f, dimensions.ElementAtOrDefault(1) / 4f);
                }

                monster.BoundingBase = new BoundingBase(h, v, VN);
            }
        }

        private static void EnrichItems()
        {
            Log.Debug("Enriching item metadata");

            //--CONNECT ITEM DATA--
            //connect item recipes
            foreach ((var itemName, var recipe) in Craft)
            {
                var item = Items[itemName];

                if (item != null)
                    item.Recipe = recipe;
            }

            //connect item ObtainableFromNPC and ExchangeAtNPC
            foreach (var npc in NPCs.Values.DistinctBy(npc => npc.Id))
            {
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

                if (npc.Token != Token.None)
                {
                    var item = Items[npc.Token];

                    //exchange at (token)
                    if (item is { ExchangeAtNPC: null })
                        item.ExchangeAtNPC = npc;
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

                //exchange at (quest)
                if ((item.ExchangeAtNPC == null) && (item.Quest != null))
                    item.ExchangeAtNPC = Quests[item.Quest.Value];
            }

            foreach ((var tokenName, var buyableItems) in Tokens)
                foreach (var itemName in buyableItems.Keys)
                {
                    var item = Items[itemName];

                    if (item is { ObtainableFromNPC: null })
                        foreach (var npc in NPCs.Values.DistinctBy(npc => npc.Id))
                            if (npc.Token.ToString().EqualsI(tokenName))
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
                    var nData = NPCs[npc.Name];
                    npc.Data = NPCs[npc.Name]!;

                    if (nData == null)
                    {
                        Log.Warn($"NPC {npc.Name} is missing metadata.");

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

                                exits.Add(new Exit(map.Accessor, location, new Location(mapAccessor, spawn), spawnId,
                                    ExitType.Transporter));
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
                        var v1 = new Point(boundary.Left, boundary.Top);
                        var v2 = new Point(boundary.Right, boundary.Bottom);
                        var boundaryMap = boundary.Map == string.Empty ? map.Accessor : boundary.Map;

                        boundaries.Add(new InscribedBoundary(v1, v2, boundaryMap));
                    }

                    if (monster._boundaries != null)
                    {
                        var mBoundaries = monster._boundaries;

                        foreach (var boundary in mBoundaries)
                        {
                            var v1 = new Point(boundary.Left, boundary.Top);
                            var v2 = new Point(boundary.Right, boundary.Bottom);
                            var boundaryMap = boundary.Map == string.Empty ? map.Accessor : boundary.Map;

                            boundaries.Add(new InscribedBoundary(v1, v2, boundaryMap));
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

                    exits.Add(new Exit(map.Accessor, door, new Location(door.DestinationMap, spawn),
                        door.DestinationSpawnId, ExitType.Door));
                }
            }
        }

        private static void EnrichMonsters()
        {
            Log.Debug("Enriching monster metadata");

            foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
            {
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
                foreach (var npc in map.NPCs)
                {
                    var nData = npc.Data;

                    if (nData == null)
                    {
                        Log.Warn($"NPC {npc.Name} is missing meetadata.");

                        continue;
                    }

                    var locations = (List<Location>)nData.Locations;
                    locations.AddRange(npc.Locations);
                }
            }
        }

        private static void EnrichQuests()
        {
            Log.Debug("Enrishing quest metadata");

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

            foreach (var recipe in Dismantle.Values)
                if (recipe.Quest.HasValue && (recipe.Quest.Value != Quest.None))
                    recipe.NPC = Quests[recipe.Quest.Value];
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
            JsonConvert.DeserializeObject<GameData>(json);

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
            //connect various data points
            EnrichRecipes();
            EnrichMaps();
            EnrichItems();
            EnrichMonsters();
            EnrichNPCs();
            BuildBoundingBases();

            stopwatch.Stop();
            Log.Info($"Serialized data in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}