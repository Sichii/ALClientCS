#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Geometry;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     <inheritdoc cref="AttributedRecordBase" />
///     <br />
///     Represents the static data for a monster.
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GMonster : AttributedRecordBase
{
    /// <summary>
    ///     If true, every hit on this monster is cut to 1 damage, or 2 on a crit (node/server.js:3618).
    /// </summary>
    [JsonPropertyName("1hp")]
    public bool _1hp { get; init; }

    /// <summary>
    ///     The abilities this monster has, indexed by the name of the ability.
    /// </summary>
    public IReadOnlyDictionary<string, GMonsterAbility> Abilities { get; init; } = new Dictionary<string, GMonsterAbility>();

    /// <summary>
    ///     The key this monster is filed under in <see cref="GameData.Monsters" />.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     Kill-count tiers that award permanent stats. Every tier at or below your kill total applies, and only
    ///     while the monster tracker is equipped (node/server.js:1331).
    /// </summary>
    public IReadOnlyList<GKillAchievement> Achievements { get; init; } = new List<GKillAchievement>();

    /// <summary>
    ///     The chance this monster joins the pool of monsters that hit passers-by without targeting them. Rolled
    ///     at most once every 1.2 seconds, and anything at or above 1 always passes (node/server.js:12791).
    /// </summary>
    public float Aggro { get; init; }

    /// <summary>
    ///     The collision footprint this monster walks and pathfinds with, from the sprite's (h, v, vn). Much
    ///     smaller than <see cref="HitBox" />, which is what a range is measured against.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public BoundingBase BoundingBase { get; set; } = null!;

    /// <summary>
    ///     The box this monster's <i>range</i> is measured against - the whole sprite, centred horizontally and rising
    ///     from its feet. Deliberately not <see cref="BoundingBase" />, which is the much smaller foot-print the game
    ///     walks and pathfinds with: an attack reaches the sprite, a step collides with the base.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public BoundingBase HitBox { get; set; } = null!;

    /// <summary>
    ///     The speed this monster moves at the moment it has a target - any target, not just a
    ///     <see cref="Rage" /> lock. It replaces <see cref="AttributedRecordBase.Speed" /> outright rather than adding to it, and is
    ///     often several times larger. A <see cref="Supporter" /> following another monster uses it too, capped
    ///     at that monster's speed plus 4 (node/server.js:1629).
    /// </summary>
    [JsonPropertyName("charge")]
    public float ChargeSpeed { get; init; }

    /// <summary>
    ///     What this monster actually chases at, which is <see cref="ChargeSpeed" /> when the design data names one
    ///     and a multiple of <see cref="AttributedRecordBase.Speed" /> when it does not. Only the explicit half is on
    ///     the wire: the game fills the rest in when it loads G (js/old_common_functions.js:162), so a monster read
    ///     straight out of the data here has a <see cref="ChargeSpeed" /> of zero rather than the speed it chases at.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public float ChaseSpeed
    {
        get
        {
            if (ChargeSpeed > 0f)
                return ChargeSpeed;

            var multiplier = Speed switch
            {
                >= 60f => 1.20f,
                >= 50f => 1.30f,
                >= 32f => 1.4f,
                >= 20f => 1.6f,
                >= 10f => 1.7f,
                _      => 2f
            };

            //the game's round(), which is half away from zero rather than the half-to-even MathF.Round does
            return MathF.Floor((Speed * multiplier) + 0.5f);
        }
    }

    /// <summary>
    ///     If true, everyone who damaged this monster is rewarded in proportion to what they contributed, rather
    ///     than only whoever it was targeting (node/server.js:2584).
    /// </summary>
    public bool Cooperative { get; init; }

    /// <summary>
    ///     If true, this monster is skipped by the timer that levels idle monsters up, and accrues no bonus gold
    ///     from the player it is fighting. Killing a player still levels it (node/server.js:14413, :12407).
    /// </summary>
    public bool Cute { get; init; }

    /// <summary>
    ///     The type of damage this monster deals.
    /// </summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     A hand-set difficulty rating. It scales the pack's gold and a player's contribution score, and a
    ///     monster rated 0 drops no gold at all (node/server.js:2136). Most monsters do not carry one.
    /// </summary>
    public float Difficulty { get; init; }

    /// <summary>
    ///     If true, every projectile fired at this monster teleports it somewhere random on the map, unless a
    ///     field generator stands within 300 units, which dampens it instead (node/server.js:3237).
    /// </summary>
    public bool Escapist { get; init; }

    /// <summary>
    ///     If true, the loot chest for this kill is placed at the killing player's own position and map rather
    ///     than the monster's (node/server.js:2142). Only the Grinch carries it.
    /// </summary>
    public bool Global { get; init; }

    /// <summary>
    ///     If true, the mana-restoring proc that "mpxgloves" grants is five times as likely against this monster
    ///     (node/server_functions.js:2806), and a <see cref="Supporter" /> only heals monsters sharing the flag.
    /// </summary>
    public bool Humanoid { get; init; }

    /// <summary>
    ///     If true, only basic attacks and the handful of skills flagged pierces_immunity damage this monster;
    ///     everything else is refused as skill_immune (node/server.js:3132). It also cannot be frozen or burned.
    /// </summary>
    public bool Immune { get; init; }

    /// <summary>
    ///     Some monsters will spawn with effects on them by default.
    /// </summary>
    [JsonPropertyName("s")]
    public IReadOnlyDictionary<Condition, GInitialCondition> InitialConditions { get; init; }
        = new Dictionary<Condition, GInitialCondition>();

    /// <summary>
    ///     The direction this monster will face when it spawns.
    /// </summary>
    [JsonPropertyName("orientation")]
    public Direction InitialDirection { get; init; }

    /// <summary>
    ///     Always zero. No monster in the game data carries this key and the server never reads it - gold comes
    ///     from the monster's own gold entry and <see cref="Difficulty" />.
    /// </summary>
    public float Lucrativeness { get; init; }

    /// <summary>
    ///     The name of the monster as seen on the GUI. (sometimes not the same as the accessor value)
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If true, hitting this monster does not make it target you (node/server.js:3864).
    /// </summary>
    public bool Passive { get; init; }

    /// <summary>
    ///     If true, this monster spawns permanently under the poisonous condition, which is what makes its hits
    ///     apply <see cref="Condition.Poisoned" /> (node/server.js:12056).
    /// </summary>
    public bool Poisonous { get; init; }

    /// <summary>
    ///     If populated, the name of the projectile this monster uses.
    /// </summary>
    public string? Projectile { get; init; }

    /// <summary>
    ///     Feeds two rolls. When this monster hits a passer-by it also locks onto them with this chance
    ///     (node/server.js:13594). Separately, once 20 seconds pass without it being attacked it gives up pursuit
    ///     with chance 1 - rage * 0.99, so a value above about 1.02 never gets bored (:12871).
    /// </summary>
    public float Rage { get; init; }

    /// <summary>
    ///     The time it takes for this monster to respawn after being killed, in seconds.
    ///     <br />
    ///     If this is -1 the monster does not respawn automatically, and neither does a <see cref="Special" /> one.
    ///     <br />
    ///     At or below 200 the wait is this many seconds plus up to 0.9s of jitter; above 200 it is 0.72x to 1.2x
    ///     this value (node/server.js:11874).
    /// </summary>
    [JsonPropertyName("respawn")]
    public float Respawn { get; init; }

    /// <summary>
    ///     A condition granted on the kill, for the condition's own default duration, to everyone who shares the
    ///     reward (node/server.js:2599).
    /// </summary>
    [JsonPropertyName("rbuff")]
    public Condition RewardBuff { get; init; }

    /// <summary>
    ///     Whether or not this monster roams outside of its initial spawn boundary. The map's own entry for it can
    ///     set the same thing, and either one is enough (node/server.js:13016).
    /// </summary>
    public bool Roam { get; init; }

    /// <summary>
    ///     A size modifier used by the GUI to display the image.
    /// </summary>
    public float Size { get; init; }

    /// <summary>
    ///     A list of areas this Monster can be found.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    public IReadOnlyList<InscribedBoundary> SpawnAreas { get; internal set; } = new List<InscribedBoundary>();

    /// <summary>
    ///     <b>
    ///         NULLABLE
    ///     </b>
    ///     . If populated, this monster spawns other monsters while it has a target, next to that target.
    ///     <br />
    ///     Each entry is the delay in milliseconds and the name of the monster spawned on it (node/server.js:12795).
    /// </summary>
    public IReadOnlyList<(float SpawnDelay, string MonsterName)>? Spawns { get; init; }

    /// <summary>
    ///     If true, this monster never respawns on its own once killed, and levels up twenty times more slowly
    ///     than an ordinary one (node/server.js:11864, :12404). Set on bosses and event monsters.
    /// </summary>
    public bool Special { get; init; }

    /// <summary>
    ///     If true, this monster does not move at all.
    /// </summary>
    public bool Stationary { get; init; }

    /// <summary>
    ///     If true, this monster follows another monster within 300 units that shares its <see cref="Humanoid" />
    ///     flag, and aims its healing ability at that one whenever it is within 120 rather than at itself
    ///     (node/server.js:12764, :12584).
    /// </summary>
    public bool Supporter { get; init; }

    /// <summary>
    ///     If true, this is a player-placed device rather than a wild monster - only the field generator and the
    ///     zapper carry it. The server tags the live entity from how it was spawned, not from this flag.
    /// </summary>
    public bool Trap { get; init; }

    /// <summary>
    ///     If true, the client leaves this monster out of its monster list (js/html.js:2400). Set on the target
    ///     dummies and one debug monster.
    /// </summary>
    public bool Unlist { get; init; }
}