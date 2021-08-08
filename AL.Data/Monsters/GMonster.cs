using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    /// <summary>
    ///     <inheritdoc cref="AttributedRecordBase" /> <br />
    ///     Represents the static data for a monster.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record GMonster : AttributedRecordBase
    {
        /// <summary>
        ///     If true, all attacks will do only 1 damage to this monster.
        /// </summary>
        [JsonProperty("1hp")]
        public bool _1hp { get; init; }

        /// <summary>
        ///     The abilities this monster has, indexed by the name of the ability.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(AttributedObjectConverter<GMonsterAbility>))]
        public IReadOnlyDictionary<string, GMonsterAbility> Abilities { get; init; } = new Dictionary<string, GMonsterAbility>();

        /// <summary>
        ///     This unique accessor for this monster.
        /// </summary>
        /// <remarks>Enriched property</remarks>
        public string Accessor { get; internal set; } = null!;

        /// <summary>
        ///     A list of the achievements associated with this monster.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<GKillAchievement>))]
        public IReadOnlyList<GKillAchievement> Achievements { get; init; } = new List<GKillAchievement>();

        /// <summary>
        ///     The chance of this monster to attack you when you enter it's range. <br />
        ///     This number is usually 0-1, but can sometimes be higher (10, 100) <br />
        ///     TODO: Figure out what 10, 100 mean
        /// </summary>
        public float Aggro { get; init; }

        /// <summary>
        ///     The bounding base of this monster. (h, v, vn)
        /// </summary>
        /// <remarks>Enriched property</remarks>
        [JsonIgnore]
        public BoundingBase BoundingBase { get; set; } = null!;

        /// <summary>
        ///     If this monster <see cref="Rage">rages</see> onto you, it will move at this speed.
        /// </summary>
        [JsonProperty("charge")]
        public float ChargeSpeed { get; init; }

        /// <summary>
        ///     If true, all participants/parties will receive a reward for this kill.
        /// </summary>
        public bool Cooperative { get; init; }

        /// <summary>
        ///     If true, this monster does not level up, even when killing players.
        /// </summary>
        public bool Cute { get; init; }

        /// <summary>
        ///     The type of damage this monster deals.
        /// </summary>
        [JsonProperty("damage_type")]
        public DamageType DamageType { get; init; }

        /// <summary>
        ///     The default estimated difficulty of this monster.
        /// </summary>
        public float Difficulty { get; init; }

        /// <summary>
        ///     If true, this monster will try to teleport away.
        /// </summary>
        public bool Escapist { get; init; }

        /// <summary>
        ///     If true, this monster can appear on any map.
        /// </summary>
        public bool Global { get; init; }

        /// <summary>
        ///     If true, the item "mpxgloves" will be 5x as effective against this monster.
        /// </summary>
        public bool Humanoid { get; init; }

        /// <summary>
        ///     If true, this monster can only be damaged by basic attacks.
        /// </summary>
        public bool Immune { get; init; }

        /// <summary>
        ///     Some monsters will spawn with effects on them by default.
        /// </summary>
        [JsonProperty("s")]
        public IReadOnlyDictionary<Condition, GInitialCondition> InitialConditions { get; init; } =
            new Dictionary<Condition, GInitialCondition>();

        /// <summary>
        ///     The direction this monster will face when it spawns.
        /// </summary>
        [JsonProperty("orientation")]
        public Direction InitialDirection { get; init; }

        /// <summary>
        ///     A multiplier for how much gold a monster drops. (probably used to calculate base_gold values)
        /// </summary>
        public float Lucrativeness { get; init; }

        /// <summary>
        ///     The name of the monster as seen on the GUI. (sometimes not the same as the accessor value)
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     If true, this monster does not attack back when attacked.
        /// </summary>
        public bool Passive { get; init; }

        /// <summary>
        ///     If true, this monster can cause <see cref="Condition.Poisoned" />.
        /// </summary>
        public bool Poisonous { get; init; }

        /// <summary>
        ///     If populated, the name of the projectile this monster uses.
        /// </summary>
        public string? Projectile { get; init; }

        /// <summary>
        ///     The chance of this monster to lock onto you. <br />
        ///     This number is usually 0-1, but can sometimes be higher (10, 100) <br />
        ///     TODO: Figure out what 10, 100 mean
        /// </summary>
        public float Rage { get; init; }

        /// <summary>
        ///     The time it takes for this monster to respawn after being killed, in seconds. <br />
        ///     If this is -1, the monster does not respawn automatically. <br />
        ///     Wizard: For >200 second respawn monsters, the variance is from 0.6 to 2.2 of their base time
        /// </summary>
        [JsonProperty("respawn")]
        public float Respawn { get; init; }

        /// <summary>
        ///     When this monster is killed, it gives this buff.
        /// </summary>
        [JsonProperty("rbuff")]
        public Condition RewardBuff { get; init; }

        /// <summary>
        ///     Whether ot not this monster roams outside of it's initial spawn boundary.
        /// </summary>
        public bool Roam { get; init; }

        /// <summary>
        ///     A size modifier used by the GUI to display the image.
        /// </summary>
        public float Size { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this monster will spawn other monsters. <br />
        ///     This list contains the delay between spawns, and the name of the monster is spawns on that delay.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<float, string>))]
        public IReadOnlyList<(float SpawnDelay, string MonsterName)>? Spawns { get; init; }

        /// <summary>
        ///     Whether or not this is a special monster. Generally means the monster is a boss/event monster, and/or is announced.
        /// </summary>
        public bool Special { get; init; }

        /// <summary>
        ///     If true, this monster does not move at all.
        /// </summary>
        public bool Stationary { get; init; }

        /// <summary>
        ///     If true, this monster will heal other monsters and itself in encounters.
        /// </summary>
        public bool Supporter { get; init; }

        /// <summary>
        ///     If true, this monster is a trap.
        /// </summary>
        public bool Trap { get; init; }

        /// <summary>
        ///     TODO: Unknown
        /// </summary>
        public bool Unlist { get; init; }
    }
}