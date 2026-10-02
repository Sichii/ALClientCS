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
    ///     The abilities this monster has, indexed by the name of the ability.
    /// </summary>
    /// <remarks>
    ///     Never null, even for the monsters whose data sends the key as null.
    /// </remarks>
    public IReadOnlyDictionary<string, GMonsterAbility> Abilities
    {
        get;

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        init => field = value ?? new Dictionary<string, GMonsterAbility>();
    } = new Dictionary<string, GMonsterAbility>();

    /// <summary>
    ///     The key this monster is filed under in <see cref="GameData.Monsters" />.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     Kill-count tiers that award permanent stats. Every tier at or below your kill total applies, and only while the
    ///     monster tracker is equipped.
    /// </summary>
    public IReadOnlyList<GKillAchievement> Achievements { get; init; } = new List<GKillAchievement>();

    /// <summary>
    ///     The chance this monster joins the pool of monsters that hit passers-by without targeting them. Rolled at most once
    ///     every 1.2 seconds, and anything at or above 1 always passes.
    /// </summary>
    public float Aggro { get; init; }

    /// <summary>
    ///     The collision footprint this monster walks and pathfinds with, from the sprite's (h, v, vn). Much smaller than
    ///     <see cref="HitBox" />, which is what a range is measured against.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    [JsonIgnore]
    public BoundingBase BoundingBase { get; set; } = null!;

    /// <summary>
    ///     The speed this monster moves at while it has any target, not just a <see cref="Rage" /> lock. It replaces
    ///     <see cref="AttributedRecordBase.Speed" /> outright and is often several times larger. A <see cref="Supporter" />
    ///     following another monster uses it too, capped at that monster's speed plus 4.
    /// </summary>
    [JsonPropertyName("charge")]
    public float ChargeSpeed { get; init; }

    /// <summary>
    ///     If true, everyone who damaged this monster is rewarded in proportion to what they contributed, rather than only
    ///     whoever it was targeting.
    /// </summary>
    public bool Cooperative { get; init; }

    /// <summary>
    ///     If true, this monster is skipped by the timer that levels idle monsters up, and accrues no bonus gold from the
    ///     player it is fighting. Killing a player still levels it.
    /// </summary>
    public bool Cute { get; init; }

    /// <summary>The type of damage this monster deals.</summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     A hand-set difficulty rating. It scales the pack's gold and a player's contribution score, and a monster rated 0
    ///     drops no gold at all. Most monsters do not carry one.
    /// </summary>
    public float Difficulty { get; init; }

    /// <summary>
    ///     If true, every projectile fired at this monster teleports it somewhere random on the map, unless a field generator
    ///     stands within 300 units, which dampens it instead.
    /// </summary>
    public bool Escapist { get; init; }

    /// <summary>
    ///     If true, the loot chest for this kill is placed at the killing player's own position and map rather than the
    ///     monster's. Only the Grinch carries it.
    /// </summary>
    public bool Global { get; init; }

    /// <summary>
    ///     The box this monster's <i>range</i> is measured against: the whole sprite, centred horizontally and rising from its
    ///     feet. Movement collides with the much smaller <see cref="BoundingBase" /> instead.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    [JsonIgnore]
    public BoundingBase HitBox { get; set; } = null!;

    /// <summary>
    ///     If true, the mana-restoring proc that <c>mpxgloves</c> grants is five times as likely against this monster and a
    ///     <see cref="Supporter" /> only heals monsters sharing the flag.
    /// </summary>
    public bool Humanoid { get; init; }

    /// <summary>
    ///     If true, only basic attacks and the handful of skills flagged <c>pierces_immunity</c> damage this monster;
    ///     everything else is refused as <c>skill_immune</c>. It also cannot be frozen or burned.
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
    ///     Always zero. No monster in the game data carries this key and the server never reads it - gold comes from the
    ///     monster's own gold entry and <see cref="Difficulty" />.
    /// </summary>
    public float Lucrativeness { get; init; }

    /// <summary>
    ///     The name of the monster as seen on the GUI. (sometimes not the same as the accessor value)
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If true, hitting this monster does not make it target you.
    /// </summary>
    public bool Passive { get; init; }

    /// <summary>
    ///     If true, this monster spawns permanently under the poisonous condition, which is what makes its hits apply
    ///     <see cref="Condition.Poisoned" />.
    /// </summary>
    public bool Poisonous { get; init; }

    /// <summary>
    ///     If populated, the name of the projectile this monster uses.
    /// </summary>
    public string? Projectile { get; init; }

    /// <summary>
    ///     The chance this monster locks onto a passer-by it hits. Once 20 seconds pass without it being attacked, it also
    ///     gives up pursuit with chance 1 - rage * 0.99.
    /// </summary>
    public float Rage { get; init; }

    /// <summary>
    ///     The time it takes for this monster to respawn after being killed, in seconds.
    ///     <br />
    ///     If this is -1 the monster does not respawn automatically, and neither does a <see cref="Special" /> one.
    ///     <br />
    ///     At or below 200 the wait is this many seconds plus up to 0.9s of jitter; above 200 it is 0.72x to 1.2x this value.
    /// </summary>
    [JsonPropertyName("respawn")]
    public float Respawn { get; init; }

    /// <summary>
    ///     A condition granted on the kill, for the condition's own default duration, to everyone who shares the reward.
    /// </summary>
    [JsonPropertyName("rbuff")]
    public Condition RewardBuff { get; init; }

    /// <summary>
    ///     Whether or not this monster roams outside of its initial spawn boundary. The map's own entry for it can set the
    ///     same thing, and either one is enough.
    /// </summary>
    public bool Roam { get; init; }

    /// <summary>
    ///     A size modifier used by the GUI to display the image.
    /// </summary>
    public float Size { get; init; }

    /// <summary>
    ///     Which cell of which sheet this monster is drawn from, as a name appearing in some
    ///     <see cref="AL.Data.Images.GSprite.Matrix" />.
    /// </summary>
    /// <remarks>
    ///     Usually the monster's own key; a nerfed or juvenile variant borrows the original's art, so this is not a name to
    ///     guess at.
    /// </remarks>
    public string? Skin { get; init; }

    /// <summary>A list of areas this Monster can be found.</summary>
    /// <remarks>Enriched property</remarks>
    public IReadOnlyList<InscribedBoundary> SpawnAreas { get; internal set; } = new List<InscribedBoundary>();

    /// <summary>
    ///     Whether any of this monster's spawn entries sets <see cref="Roam" />.
    /// </summary>
    /// <remarks>Enriched property</remarks>
    public bool SpawnRoams { get; internal set; }

    /// <summary>
    ///     <b>NULLABLE</b>. If populated, this monster spawns other monsters while it has a target, next to that target.
    ///     <br />
    ///     Each entry is the delay in milliseconds and the name of the monster spawned on it.
    /// </summary>
    public IReadOnlyList<(float SpawnDelay, string MonsterName)>? Spawns { get; init; }

    /// <summary>
    ///     If true, this monster never respawns on its own once killed, and levels up twenty times more slowly than an
    ///     ordinary one. Set on bosses and event monsters.
    /// </summary>
    public bool Special { get; init; }

    /// <summary>If true, this monster does not move at all.</summary>
    public bool Stationary { get; init; }

    /// <summary>
    ///     If true, this monster follows another monster within 300 units that shares its <see cref="Humanoid" /> flag, and
    ///     aims its healing ability at that one whenever it is within 120 rather than at itself.
    /// </summary>
    public bool Supporter { get; init; }

    /// <summary>
    ///     If true, this is a player-placed device rather than a wild monster; only the field generator and the zapper carry
    ///     it. The server tags the live entity from how it was spawned, not from this flag.
    /// </summary>
    public bool Trap { get; init; }

    /// <summary>
    ///     If true, the client leaves this monster out of its monster list. Set on the target dummies and one debug monster.
    /// </summary>
    public bool Unlist { get; init; }

    /// <summary>
    ///     If true, every hit on this monster is cut to 1 damage, or 2 on a crit.
    /// </summary>
    [JsonPropertyName("1hp")]
    public bool _1hp { get; init; }

    /// <summary>
    ///     The speed this monster chases at: <see cref="ChargeSpeed" /> when the data names one, otherwise a multiple of
    ///     <see cref="AttributedRecordBase.Speed" />, as the game fills it in when it loads G.
    /// </summary>
    /// <remarks>Enriched property</remarks>
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
            return MathF.Floor(Speed * multiplier + 0.5f);
        }
    }

    /// <summary>
    ///     Whether this monster roams anywhere on its map, from either its own flag or a spawn entry's.
    /// </summary>
    [JsonIgnore]
    public bool Roams => Roam || SpawnRoams;
}