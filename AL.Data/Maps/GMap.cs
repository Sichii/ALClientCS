using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.Data.Geometry;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    /// <summary>
    ///     Represents the static info of a map.
    /// </summary>
    public record GMap
    {
        /// <summary>
        ///     The unique accessor for this map.
        /// </summary>
        [JsonIgnore]
        public string Accessor { get; internal set; } = null!;
        /// <summary>
        ///     If true, the map has no walls.
        /// </summary>
        [JsonProperty("no_bounds")]
        public bool Boundless { get; init; }

        /// <summary>
        ///     Certain maps have a multiplier for the chance for <see cref="Condition.Burned" /> to apply.
        /// </summary>
        [JsonProperty("burn_multiplier")]
        public float BurnMultiplier { get; init; }

        /// <summary>
        ///     A list of doors on this map,
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<GDoor>))]
        public IReadOnlyList<GDoor> Doors { get; init; } = new List<GDoor>();

        /// <summary>
        ///     <b>Currently not used.</b> <br />
        ///     A value used to determine how often items drop.
        /// </summary>
        [JsonProperty("drop_norm")]
        public float DropNorm { get; init; }

        /// <summary>
        ///     If this is an event map, this will contain the name of the event this map is for.
        /// </summary>
        public string? Event { get; init; }

        /// <summary>
        ///     A list of exits on this map. Exits can be either doors, or npcs that transport you.
        /// </summary>
        /// <remarks>Enriched property</remarks>
        [JsonIgnore]
        public IReadOnlyList<Exit> Exits { get; internal set; } = new List<Exit>();

        /// <summary>
        ///     Certain maps have a multiplier for the chance for <see cref="Condition.Frozen" /> to apply.
        /// </summary>
        [JsonProperty("freeze_multiplier")]
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
        ///     Whether or not this map is irregular. You must "leave" irregular maps, rather than use a door.
        /// </summary>
        public bool Irregular { get; init; }

        /// <summary>
        ///     An internal key for the map. (not the same as the string used to access this map object) <br />
        ///     TODO: Is this used?
        /// </summary>
        public string Key { get; init; } = null!;

        /// <summary>
        ///     Whether or not you can lose items/gold/exp when dying.
        /// </summary>
        public bool Loss { get; init; }

        /// <summary>
        ///     A list of monsters that spawn on this map, as well as the amount that spawn, and where they spawn.
        /// </summary>
        public IReadOnlyList<GMapMonster> Monsters { get; init; } = new List<GMapMonster>();

        /// <summary>
        ///     Seems only one character on your account can be in a map with this flag at a time?
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
        [JsonProperty("on_death"), JsonConverter(typeof(ArrayToTupleConverter<string, float>))]
        public (string Map, float Spawn) OnDeath { get; init; }

        /// <summary>
        ///     The map accessor and spawn id for that map that you will go to if you "exit" this map. <br />
        ///     Wizard - "for example when you reload inside the bank or an instance,
        ///     it specifies which map/spawn you'll spawn at (in case that instance is no longer alive, you can't re-enter etc.)"
        /// </summary>
        [JsonProperty("on_exit"), JsonConverter(typeof(ArrayToTupleConverter<string, float>))]
        public (string Map, float Spawn) OnExit { get; init; }

        /// <summary>
        ///     Whether or not PvP is allowed on this map.
        /// </summary>
        public bool PvP { get; init; }

        /// <summary>
        ///     If true, monsters do not spawn on this map.
        /// </summary>
        public bool Safe { get; init; }

        /// <summary>
        ///     TODO: Unknown
        /// </summary>
        [JsonProperty("safe_pvp")]
        public bool SafePvP { get; init; }

        /// <summary>
        ///     A list of spawns on this map.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<GSpawn>))]
        public IReadOnlyList<GSpawn> Spawns { get; init; } = new List<GSpawn>();

        /// <summary>
        ///     A list of traps on this map, as well as where they spawn, and their type.
        /// </summary>
        public IReadOnlyList<GTrap> Traps { get; init; } = new List<GTrap>();

        /// <summary>
        ///     TODO: Unknown
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
        ///     A list of special zones on the map, and where they are. <br />
        ///     Zones are for things like fishing and mining.
        /// </summary>
        public IReadOnlyList<GZone> Zones { get; init; } = new List<GZone>();

        //quirks
        //animatables obj
        //machines obj[]
        //ref obj
        //old_monsters obj[]

        public virtual bool Equals(GMap? other) =>
            other is not null && Accessor.EqualsI(other.Accessor);

        public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), Key.GetHashCode());
    }
}