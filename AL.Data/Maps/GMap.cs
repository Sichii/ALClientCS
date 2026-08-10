#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Data.Geometry;
using Chaos.Extensions.Common;
#endregion

namespace AL.Data.Maps;

/// <summary>
///     Represents the static info of a map.
/// </summary>
public sealed record GMap
{
    /// <summary>
    ///     The unique accessor for this map.
    /// </summary>
    [JsonIgnore]
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     If true, the server builds no walkable-area map for this one, so no position is out of bounds and no
    ///     move can be jailed for landing off it (node/server_functions.js:4010).
    /// </summary>
    [JsonPropertyName("no_bounds")]
    public bool Boundless { get; init; }

    /// <summary>
    ///     Scales the chance an attacker's burn proc lands here (node/server.js:3202). Zero means the map sets
    ///     none, which the server reads as 1.
    /// </summary>
    [JsonPropertyName("burn_multiplier")]
    public float BurnMultiplier { get; init; }

    /// <summary>
    ///     A list of doors on this map,
    /// </summary>
    public IReadOnlyList<GDoor> Doors { get; init; } = new List<GDoor>();

    /// <summary>
    ///     <b>
    ///         Currently not used.
    ///     </b>
    ///     <br />
    ///     A value used to determine how often items drop. The server hardcoded it to 1000 for every map and left
    ///     a note saying so (node/server.js:2106).
    /// </summary>
    [JsonPropertyName("drop_norm")]
    public float DropNorm { get; init; }

    /// <summary>
    ///     If this is an event map, this will contain the name of the event this map is for.
    /// </summary>
    public string? Event { get; init; }

    /// <summary>
    ///     A list of exits on this map. Exits can be either doors, or npcs that transport you.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyList<Exit> Exits { get; internal set; } = new List<Exit>();

    /// <summary>
    ///     Scales the chance an attacker's freeze proc lands here (node/server.js:3187). Zero means the map sets
    ///     none, which the server reads as 1.
    /// </summary>
    [JsonPropertyName("freeze_multiplier")]
    public float FreezeMultiplier { get; init; }

    /// <summary>
    ///     The name of the special effect on the map, if it has one. Will also have <see cref="Weather" />.
    /// </summary>
    public string? FX { get; init; }

    /// <summary>
    ///     If populated, an object containing information about the geometry for this map.
    /// </summary>
    public GGeometry? Geomertry { get; internal set; }

    /// <summary>
    ///     If this is true, this is bad/old data that should be ignored.
    /// </summary>
    public bool Ignore { get; init; }

    /// <summary>
    ///     Whether or not this map is an instance.
    /// </summary>
    public bool Instance { get; init; }

    /// <summary>
    ///     If true, the client leaves this map out of its travel list and the server skips it when scanning every
    ///     map for monsters (js/html.js:4960, node/server.js:4950). Jail, the duel and code lands, the resort and
    ///     the test map carry it.
    /// </summary>
    public bool Irregular { get; init; }

    /// <summary>
    ///     Names the map's geometry document in the server's database, as "MP_" plus this value
    ///     (node/server.js:392). Not the string used to access this map object.
    /// </summary>
    public string Key { get; init; } = null!;

    /// <summary>
    ///     Dead. Nothing in the server or the official client reads it, and the one map that declares it declares
    ///     it false, so this is always false.
    /// </summary>
    public bool Loss { get; init; }

    /// <summary>
    ///     A list of monsters that spawn on this map, as well as the amount that spawn, and where they spawn.
    /// </summary>
    public IReadOnlyList<GMapMonster> Monsters { get; init; } = new List<GMapMonster>();

    /// <summary>
    ///     Marks a bank level. Entering mounts the account-wide bank onto the character and leaving unmounts it,
    ///     both asynchronous, and a second attempt while one is in flight fails as "bank_opi"
    ///     (node/server.js:5449).
    /// </summary>
    public bool Mount { get; init; }

    /// <summary>
    ///     The name of this map. (not the same as the string used to access this map object)
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     A list of npcs on this map, as well as where they are located.
    /// </summary>
    public IReadOnlyList<GMapNPC> NPCs { get; init; } = new List<GMapNPC>();

    /// <summary>
    ///     The map accessor and spawn id for that map that you will go to if you die on this map.
    /// </summary>
    [JsonPropertyName("on_death")]
    public (string Map, float Spawn) OnDeath { get; init; }

    /// <summary>
    ///     The map accessor and spawn id for that map that you will go to if you "exit" this map.
    ///     <br />
    ///     Wizard - "for example when you reload inside the bank or an instance, it specifies which map/spawn you'll spawn at
    ///     (in case that instance is no longer alive, you can't re-enter etc.)"
    /// </summary>
    [JsonPropertyName("on_exit")]
    public (string Map, float Spawn) OnExit { get; init; }

    /// <summary>
    ///     Whether or not PvP is allowed on this map.
    /// </summary>
    public bool PvP { get; init; }

    /// <summary>
    ///     If true, nothing hostile can be done here: a player's attack is refused as "friendly" and a hostile
    ///     skill as "skill_cant_safe" (node/server.js:3139, :8930). These maps also carry no monsters.
    /// </summary>
    public bool Safe { get; init; }

    /// <summary>
    ///     Softens the stakes of PvP on this map. Only applies while the server itself is not a PvP server.
    /// </summary>
    /// <remarks>
    ///     A kill here transfers no gold and costs the loser no xp, and monster packs pay double gold as they do on
    ///     any PvP map. Item loss is not softened: this flag never feeds the server's own PvP test, so a kill still
    ///     drops recently looted items, and that mark only clears on banking or at the next login once an hour old.
    ///     Dying to a monster costs full xp here, because the tenfold discount other PvP maps grant is cancelled.
    /// </remarks>
    [JsonPropertyName("safe_pvp")]
    public bool SafePvP { get; init; }

    /// <summary>
    ///     A list of spawns on this map.
    /// </summary>
    public IReadOnlyList<GSpawn> Spawns { get; init; } = new List<GSpawn>();

    /// <summary>
    ///     A list of traps on this map, as well as where they spawn, and their type.
    /// </summary>
    public IReadOnlyList<GTrap> Traps { get; init; } = new List<GTrap>();

    /// <summary>
    ///     If true, the client leaves this map out of its travel list, though it is a perfectly ordinary map
    ///     otherwise (js/html.js:4959).
    /// </summary>
    public bool Unlist { get; init; }

    /// <summary>
    ///     The type of weather on this map, if it has any. Will also have <see cref="FX" />.
    /// </summary>
    public string? Weather { get; init; }

    /// <summary>
    ///     Currently only used to specify a map is part of Dungeon World.
    /// </summary>
    public WorldType World { get; init; }

    /// <summary>
    ///     A list of special zones on the map, and where they are.
    ///     <br />
    ///     Zones are for things like fishing and mining.
    /// </summary>
    public IReadOnlyList<GZone> Zones { get; init; } = new List<GZone>();

    //quirks
    //animatables obj
    //machines obj[]
    //ref obj
    //old_monsters obj[]

    public bool Equals(GMap? other) => other is not null && Accessor.EqualsI(other.Accessor);

    public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), Key.GetHashCode());
}