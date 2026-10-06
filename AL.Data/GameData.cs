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
using AL.Data.Compounds;
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
using AL.Data.MonsterGold;
using AL.Data.Monsters;
using AL.Data.Multipliers;
using AL.Data.NPCs;
using AL.Data.Projectiles;
using AL.Data.Sets;
using AL.Data.Skills;
using AL.Data.Titles;
using AL.Data.Tokens;
using AL.Data.Upgrades;
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
    ///     The cosmetics every account may wear whether it owns them or not, the server's own <c>free_cx</c>.
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
    ///     The game-data version the data members were last generated against. AL.MemberGenerator writes it to
    ///     <c>dataMembers/version.txt</c>.
    /// </summary>
    public const int KNOWN_VERSION = 17625;

    /// <summary>
    ///     The hit box every player is measured against for range: 26 wide and 36 tall, fixed for everyone. Their collision
    ///     box is the much smaller pathfinding default.
    /// </summary>
    public static readonly BoundingBase DEFAULT_CHARACTER_HIT_BOX = new(13f, 36f, 0f);

    /// <summary>
    ///     Serializes the runtime edits to the map and geometry tables against each other. Reads need nothing, because each
    ///     edit swaps in a fresh copy of the table rather than mutating the one a reader holds.
    /// </summary>
    private static readonly Lock GeneratedLock = new();

    [GameDataRoot]
    public static AchievementsDatum Achievements { get; private set; }

    [GameDataRoot]
    public static ClassesDatum Classes { get; private set; }

    [GameDataRoot]
    public static CompoundsDatum Compounds { get; private set; }

    [GameDataRoot]
    public static ConditionsDatum Conditions { get; private set; }

    /// <summary>
    ///     The wardrobe rules. Empty tables when the payload carries no <c>cosmetics</c>.
    /// </summary>
    [GameDataRoot]
    public static GCosmetics Cosmetics { get; private set; } = new();

    [GameDataRoot]
    public static CraftDatum Craft { get; private set; }

    [GameDataRoot]
    public static DimensionsDatum Dimensions { get; private set; }

    [GameDataRoot]
    public static DismantleDatum Dismantle { get; private set; }

    /// <summary>
    ///     The drop tables. Empty when the payload carries no <c>drops</c>.
    /// </summary>
    [GameDataRoot]
    public static GDrops Drops
    {
        get;

        [UsedImplicitly]
        private set;
    } = new();

    [GameDataRoot]
    public static EventsDatum Events { get; private set; }

    [GameDataRoot]
    public static GamesDatum Games { get; private set; }

    [GameDataRoot]
    public static GeometryDatum Geometry { get; private set; }

    [GameDataRoot]
    public static IReadOnlyDictionary<string, GImageSet> ImageSets { get; private set; } = new Dictionary<string, GImageSet>();

    /// <summary>
    ///     The pixel size of each asset file, keyed by path. Empty when the payload carries none.
    /// </summary>
    [GameDataRoot]
    public static IReadOnlyDictionary<string, GImage> Images { get; private set; } = new Dictionary<string, GImage>();

    [GameDataRoot]
    public static ItemsDatum Items { get; private set; }

    [GameDataRoot]
    public static IReadOnlyDictionary<int, float> Levels { get; private set; } = new Dictionary<int, float>();

    [GameDataRoot]
    public static MapsDatum Maps { get; private set; }

    [GameDataRoot]
    [JsonPropertyName("monster_gold")]
    public static MonsterGoldDatum MonsterGold { get; private set; }

    [GameDataRoot]
    public static MonstersDatum Monsters { get; private set; }

    /// <summary>
    ///     The game's economy ratios. Zeroed when the payload carries no <c>multipliers</c>.
    /// </summary>
    [GameDataRoot]
    public static GMultipliers Multipliers
    {
        get;

        [UsedImplicitly]
        private set;
    } = new();

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
    public static UpgradesDatum Upgrades { get; private set; }

    [GameDataRoot]
    public static int Version { get; private set; }

    [JsonIgnore]
    public static int ShellsToGold => Multipliers.ShellsToGold;

    /// <summary>
    ///     Folds every map's composed scenery into its wall lines, the way the game does while it processes a map.
    /// </summary>
    private static void AddAnimatableWalls()
    {
        foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
            AddAnimatableWalls(map);
    }

    /// <summary>
    ///     Turns one map's scenery collision boxes into wall lines and appends them to its geometry.
    /// </summary>
    /// <remarks>
    ///     The game does this while processing the map rather than shipping the lines in <c>G.geometry</c>. Duplicates are
    ///     left to <see cref="FixLines(GGeometry)" />.
    /// </remarks>
    /// <param name="map">The map whose scenery to fold in.</param>
    private static void AddAnimatableWalls(GMap map)
    {
        if (map.Animatables.Count == 0)
            return;

        if (Geometry[map.Accessor] is not { } mapGeometry)
            return;

        var horizontalLines = (List<StraightLine>)mapGeometry.HorizontalLines;
        var verticalLines = (List<StraightLine>)mapGeometry.VerticalLines;

        foreach (var line in map.Animatables.Values.SelectMany(animatable => animatable.BuildCollisionLines()))
            if (line.IsVertical)
                verticalLines.Add(line);
            else
                horizontalLines.Add(line);
    }

    private static void AddBorderWalls()
    {
        foreach (var mapGeometry in Geometry.Values.DistinctBy(mapGeometry => mapGeometry.Accessor))
            AddBorderWalls(mapGeometry);
    }

    private static void AddBorderWalls(GGeometry mapGeometry)
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

    /// <summary>
    ///     Binds every <see cref="GameDataRootAttribute" /> static from the payload by reflection. Wire keys match
    ///     case-insensitively, and an absent key leaves the member's own initializer intact.
    /// </summary>
    /// <param name="json">The game data payload.</param>
    /// <returns>The parsed payload.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The payload is not a JSON object, or a root has no setter.
    /// </exception>
    private static JsonObject Bind(string json)
    {
        var root = JsonNode.Parse(json)
                           ?.AsObject()
                   ?? throw new InvalidOperationException("Game data is not a JSON object.");

        var members = typeof(GameData).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                      .Where(property => property.GetCustomAttribute<GameDataRootAttribute>() is not null);

        foreach (var member in members)
        {
            //a get-only root would silently keep its initializer, so it is a declaration error
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

    /// <summary>
    ///     Builds the class and map bonuses from an item's unbound keys: every object filed under a class's or a map's key.
    /// </summary>
    /// <remarks>
    ///     An upgrade or compound line inside a bonus only reaches the stats the bonus also names at its own top level.
    /// </remarks>
    /// <param name="wireExtras">The item's unbound keys.</param>
    /// <returns>The bonuses, keyed by class or map.</returns>
    private static IReadOnlyDictionary<string, GItemBonus> BuildBonuses(Dictionary<string, JsonElement> wireExtras)
    {
        var bonuses = new Dictionary<string, GItemBonus>(StringComparer.OrdinalIgnoreCase);

        foreach ((var key, var element) in wireExtras)
        {
            if (element.ValueKind != JsonValueKind.Object)
                continue;

            var namesClass = EnumHelper.TryParse(key, out ALClass alClass) && Classes[alClass] is not null;

            if (!namesClass && Maps[key] is null)
                continue;

            if (element.Deserialize<GItemBonus>(ALJson.Options) is not { } bonus)
                continue;

            bonuses[key] = bonus with
            {
                UpgradeModifiers = FilterToNamedStats(bonus.UpgradeModifiers, bonus),
                CompoundModifiers = FilterToNamedStats(bonus.CompoundModifiers, bonus)
            };
        }

        return bonuses;

        static IReadOnlyDictionary<ALAttribute, float>? FilterToNamedStats(IReadOnlyDictionary<ALAttribute, float>? line, GItemBonus bonus)
            => line?.Where(entry => bonus.Attributes.ContainsKey(entry.Key))
                   .ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public static void BuildBoundingBases()
    {
        const float UNSIZED_HIT_BOX = 24f;

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

            //the collision box the game walks and pathfinds with
            monster.BoundingBase = new BoundingBase(h, v, VN);

            //the hit box every range check is resolved against: the whole sprite, scaled by the size multiplier
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

    /// <summary>
    ///     Builds every prize table this item can be exchanged for, by level.
    /// </summary>
    /// <param name="item">The exchangeable item.</param>
    /// <returns>
    ///     The prize tables keyed by level, or <c>null</c> where the data has none.
    /// </returns>
    private static IReadOnlyDictionary<int, IReadOnlyList<GDrop>>? BuildExchangeRewards(GItem item)
    {
        //the game's grade tables stop at 12
        const int MAX_EXCHANGE_LEVEL = 12;

        //an item that neither compounds nor upgrades keys its table by its bare name
        if (item is { CompoundModifiers: null, UpgradeModifiers: null })
            return Drops.Tables.GetValueOrDefault(item.Accessor) is { } table
                ? new Dictionary<int, IReadOnlyList<GDrop>>
                {
                    [0] = table
                }
                : null;

        var levelled = new Dictionary<int, IReadOnlyList<GDrop>>();

        //otherwise the table is keyed by the item's name plus its level
        for (var level = 0; level <= MAX_EXCHANGE_LEVEL; level++)
            if (Drops.Tables.GetValueOrDefault(item.Accessor + level) is { } table)
                levelled[level] = table;

        return levelled.Count > 0 ? levelled : null;
    }

    /// <summary>
    ///     Calculates where the server lets a door open from, as a band plus a range. The band is a door-sized box standing on
    ///     the door's own spawn, grown by the character's box.
    /// </summary>
    /// <param name="map">The map the door is on.</param>
    /// <param name="door">The door.</param>
    /// <returns>
    ///     The band and range, or a zero-size band on the door with no range when the door's spawn cannot be resolved.
    /// </returns>
    private static (Rectangle Band, float Range) CalculateDoorReach(GMap map, GDoor door)
    {
        var spawnId = (int)door.CurrentMapSpawnId;

        if ((spawnId < 0) || (spawnId >= map.Spawns.Count))
            return (new Rectangle(
                door.X,
                door.Y,
                0f,
                0f), 0f);

        var spawn = map.Spawns[spawnId];

        //a generated floor's stairs open within the stair range of the landing
        if (map.Generated is not null)
            return (new Rectangle(
                spawn.X,
                spawn.Y,
                0f,
                0f), CONSTANTS.STAIR_RANGE);

        var halfWidth = door.Width / 2 + CONSTANTS.CHARACTER_BOX_WIDTH / 2;
        var top = spawn.Y - door.Height;
        var bottom = spawn.Y + CONSTANTS.CHARACTER_BOX_HEIGHT;

        var band = new Rectangle(new Point(spawn.X - halfWidth, top), new Point(spawn.X + halfWidth, bottom));

        //a door outside its own band is paired with the wrong spawn, so walk to the door itself
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
    ///     Removes wall geometry inside the rect and walls off its long sides, leaving a walkable vertical corridor connecting
    ///     whatever the rect's two short ends overlap.
    /// </summary>
    /// <param name="mapAccessor">The map to carve.</param>
    /// <param name="left">The rect's left edge.</param>
    /// <param name="right">The rect's right edge.</param>
    /// <param name="top">The rect's top edge.</param>
    /// <param name="bottom">The rect's bottom edge.</param>
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

    /// <summary>
    ///     Carves the corridors that exist only in local data.
    /// </summary>
    /// <remarks>
    ///     The server checks only a move's endpoints against its walkable lattice, never the segment between them, so a carved
    ///     channel routes a crossing the game's own geometry forbids.
    /// </remarks>
    private static void CarveCorridors()

        //winterland ice golem island, across the lake's narrowest water
        => CarveCorridor(
            "winterland",
            733,
            755,
            272,
            352);

    /// <summary>
    ///     Drops the portion of each line inside the window: a line strictly between the on-axis bounds is clipped to the span
    ///     bounds, splitting into up to two pieces. Lines on the window edge merge with the seals instead.
    /// </summary>
    /// <param name="lines">The lines to clip.</param>
    /// <param name="onMin">
    ///     The window's lower bound on the lines' fixed axis.
    /// </param>
    /// <param name="onMax">
    ///     The window's upper bound on the lines' fixed axis.
    /// </param>
    /// <param name="spanMin">The window's lower bound along the lines.</param>
    /// <param name="spanMax">The window's upper bound along the lines.</param>
    /// <returns>
    ///     The lines, with every portion inside the window removed.
    /// </returns>
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
    ///     Counts wire members across the datum-backed roots that no generated property declares, which means
    ///     AL.MemberGenerator needs a re-run.
    /// </summary>
    /// <param name="root">The parsed payload.</param>
    /// <returns>The number of undeclared members.</returns>
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
    ///     Finishes every class's exclusive-cosmetic list the way the server's own game-data pass does: the free makeups, then
    ///     the name and every per-slot piece of each of the class's <see cref="GClassLook" />s.
    /// </summary>
    private static void EnrichClasses()
    {
        Log.Debug("Enriching class cosmetics");

        foreach (var gClass in Classes.Values)
        {
            var exclusives = new List<string>(gClass.ExclusiveCosmetics);

            foreach (var cosmetic in FREE_COSMETICS)
                if (!exclusives.Contains(cosmetic))
                    exclusives.Add(cosmetic);

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
            //skip anything that is not a list of drop entries
            if (element.ValueKind != JsonValueKind.Array)
                continue;

            if (element.Deserialize<IReadOnlyList<GDrop>>() is { } table)
                tables[dropId] = table;
        }

        Drops.Tables = tables;
    }

    private static void EnrichItems()
    {
        //the npc every exchange with no quest tag of its own is made at
        const string EXCHANGE_NPC = "exchange";

        Log.Debug("Enriching item metadata");

        //--CONNECT ITEM DATA--
        //connect item recipes
        foreach ((var itemName, var recipe) in Craft.Entries)
        {
            var item = Items[itemName];

            if (item != null)
                item.Recipe = recipe;
        }

        //connect item ObtainableFromNPC. first writer wins, so placed sellers go first
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
            //the wire keys are cleared once read
            if (item.WireExtras is not null)
            {
                item.Bonuses = BuildBonuses(item.WireExtras);
                item.WireExtras = null;
            }

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

            //exchange at the item's quest npc when it carries a quest tag, and the fixed exchange npc otherwise
            if (item.ExchangeCount.HasValue)
            {
                item.ExchangeAtNPC = item.Quest is { } quest ? Quests.GetValueOrDefault(quest) : NPCs[EXCHANGE_NPC];

                item.ExchangeRewards = BuildExchangeRewards(item);
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

    /// <summary>
    ///     Applies one map's share of <see cref="EnrichMaps" />, so a floor filed at runtime gets the same exits, npc and
    ///     monster links G's own maps got on load.
    /// </summary>
    /// <param name="map">The map to enrich.</param>
    private static void EnrichMap(GMap map)
    {
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
            (var band, var range) = CalculateDoorReach(map, door);

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

    private static void EnrichMaps()
    {
        Log.Debug("Enriching map metadata");

        //--CONNECT MAP DATA--
        foreach (var map in Maps.Values.DistinctBy(map => map.Accessor))
        {
            if (map.Ignore)
                continue;

            EnrichMap(map);
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

                if (monster.Roam)
                    mData.SpawnRoams = true;
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

        //the server requires the craftsman for every dismantle
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
    ///     The server folds its own copy at boot, but the downloaded data carries per-tier deltas. A count past the top
    ///     authored tier keeps the top total.
    /// </remarks>
    private static void EnrichSets()
    {
        foreach (var set in Sets.Values)
        {
            //the wire tiers are cleared once folded, and the lookup can file one set under several keys
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

                        //copied, since the running total keeps being added to
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

    private static GMap FileGeneratedFloor(GeneratedFloor floor)
    {
        var map = floor.Definition;
        map.Accessor = floor.Key;

        if (string.IsNullOrEmpty(map.Key))
            map.Key = floor.Key;

        Maps.Add(floor.Key, map);

        return map;
    }

    private static void FixLines()
    {
        Log.Debug("Merging overlapped lines");

        foreach (var mapGeometry in Geometry.Values.DistinctBy(mapGeometry => mapGeometry.Accessor))
            FixLines(mapGeometry);
    }

    private static void FixLines(GGeometry mapGeometry)
    {
        mapGeometry.VerticalLines = LineHelper.FixLines(mapGeometry.VerticalLines, true);
        mapGeometry.HorizontalLines = LineHelper.FixLines(mapGeometry.HorizontalLines, false);
    }

    public static void Populate(string json)
    {
        var stopwatch = Stopwatch.StartNew();

        Log.Info("Deserializing game data");
        var root = Bind(json);

        //the data members only need regenerating when the live data carries members they do not declare
        if (Version > KNOWN_VERSION)
        {
            var unknownMembers = CountUnknownMembers(root);

            if (unknownMembers > 0)
                Log.Warn(
                    $"Server game data is version {Version}, newer than the version the data members were generated against"
                    + $" ({KNOWN_VERSION}), and carries {unknownMembers} members they do not declare."
                    + " Re-run AL.MemberGenerator.");
        }

        Log.Info("Constructing data lookups");

        //a section the payload lacks reads as an empty table rather than a null root
        // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        Compounds ??= new CompoundsDatum();
        MonsterGold ??= new MonsterGoldDatum();
        Upgrades ??= new UpgradesDatum();

        // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

        Achievements.BuildLookupTable();
        Classes.BuildLookupTable();
        Compounds.BuildLookupTable();
        Conditions.BuildLookupTable();
        Craft.BuildLookupTable();
        Dimensions.BuildLookupTable();
        Dismantle.BuildLookupTable();
        Events.BuildLookupTable();
        Geometry.BuildLookupTable();
        Items.BuildLookupTable();
        Maps.BuildLookupTable();
        MonsterGold.BuildLookupTable();
        Monsters.BuildLookupTable();
        NPCs.BuildLookupTable();
        Projectiles.BuildLookupTable();
        Sets.BuildLookupTable();
        Skills.BuildLookupTable();
        Titles.BuildLookupTable();
        Tokens.BuildLookupTable();
        Upgrades.BuildLookupTable();

        //fix line data (merge lines, set isX for x lines). scenery first, so its boxes go through the same merge
        AddAnimatableWalls();
        AddBorderWalls();
        FixLines();

        //local-only geometry edits: open walkable corridors through walls the server tolerates crossing
        CarveCorridors();

        Log.Info("Enriching data");

        //populate quest dictionary with npcs
        EnrichQuests();

        //drops first, since the map and item passes read the tables it builds
        EnrichDrops();

        //connect various data points. NPCs before items, since the item pass prefers a placed seller and
        //GNPC.Locations is empty until EnrichNPCs fills it
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

    /// <summary>
    ///     Files a dungeon run's floors under their keys, enriched like the maps G carries. A manifest entry files the floor's
    ///     record alone, so a stair leading to it resolves before its geometry arrives. Delivering a floor twice changes
    ///     nothing.
    /// </summary>
    /// <param name="bundle">The run's manifest and delivered floors.</param>
    /// <exception cref="ArgumentNullException">bundle</exception>
    public static void RegisterGeneratedFloors(GeneratedMapBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        lock (GeneratedLock)
        {
            var filed = new List<GMap>();

            foreach (var entry in bundle.Manifest)
            {
                if (Maps[entry.Key] is not null)
                    continue;

                filed.Add(FileGeneratedFloor(entry));
            }

            foreach (var floor in bundle.Floors)
            {
                if (Geometry[floor.Key] is not null)
                    continue;

                var geometry = floor.Geometry!;
                geometry.Accessor = floor.Key;
                AddBorderWalls(geometry);
                FixLines(geometry);
                Geometry.Add(floor.Key, geometry);

                filed.Add(FileGeneratedFloor(floor));
            }

            //after every record is filed, so a stair between two floors of one bundle resolves either way round
            foreach (var map in filed)
                EnrichMap(map);
        }
    }

    /// <summary>
    ///     Takes a run's floors back out of the map and geometry tables.
    /// </summary>
    /// <param name="run">The run's id.</param>
    /// <exception cref="ArgumentNullException">run</exception>
    public static void UnregisterGeneratedRun(string run)
    {
        ArgumentNullException.ThrowIfNull(run);

        lock (GeneratedLock)
        {
            var keys = Maps.Entries
                           .Where(kvp => kvp.Value.Generated is { } generated && run.EqualsI(generated.Run))
                           .Select(kvp => kvp.Key)
                           .ToList();

            foreach (var key in keys)
            {
                Maps.Remove(key);
                Geometry.Remove(key);
            }
        }
    }
}