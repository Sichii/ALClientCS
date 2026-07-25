#region
using AL.Core.Definitions;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when an entity is hit.
/// </summary>
/// <remarks>
///     The server emits six mutually incompatible shapes on this event (normal, reflect, evade, miss, avoid and
///     burn tick), so every field below the near-universal hid/id/source is optional.
/// </remarks>
public sealed record HitData
{
    /// <summary>
    ///     GUI related. If populated, name of the animation played for this hit.
    /// </summary>
    [JsonProperty("anim")]
    public string? Animation { get; init; }

    /// <summary>
    ///     Whether or not this hit was part of an area-of-effect attack such as cleave or shadowstrike.
    /// </summary>
    [JsonProperty("aoe")]
    public bool? AOE { get; init; }

    /// <summary>
    ///     TODO: Unsure, I see it a lot for heal projectiles when the target moves a long distance away from their initial
    ///     location (so they avoided the projectile?), but the target still gets healed.
    /// </summary>
    public bool Avoid { get; init; }

    /// <summary>
    ///     If populated, the critical damage multiplier applied to this hit. This is a multiplier, not a damage figure.
    /// </summary>
    public float? Crit { get; init; }

    /// <summary>
    ///     The amount of damage this hit did.
    /// </summary>
    public int Damage { get; init; }

    /// <summary>
    ///     If populated, the type of damage this hit dealt.
    /// </summary>
    [JsonProperty("damage_type")]
    public DamageType? DamageType { get; init; }

    /// <summary>
    ///     The amount of damage returned to the attacker from this hit.
    ///     <br />
    ///     DReturn is <see cref="AL.Core.Definitions.DamageType.Physical" /> damage only, and this number ignores the source
    ///     entity's armor because this number results from calculating damage on the target entity using it's armor.
    /// </summary>
    public float DReturn { get; init; }

    /// <summary>
    ///     Whether or not the hit was evaded by <see cref="AL.Core.Definitions.ALAttribute.Evasion" />.
    /// </summary>
    public bool Evade { get; init; }

    /// <summary>
    ///     If populated, the amount of gold moved by this hit. Positive when the attacker stole it, negative when the
    ///     target gained it.
    /// </summary>
    [JsonProperty("goldsteal")]
    public float? GoldSteal { get; init; }

    /// <summary>
    ///     If populated, the amount of health restored to the target by this hit.
    /// </summary>
    public float? Heal { get; init; }

    /// <summary>
    ///     The ID of the entity that dealt the hit, not an identifier for the hit itself.
    /// </summary>
    [JsonProperty("hid")]
    public string HitId { get; init; } = null!;

    /// <summary>
    ///     The ID of the entity that got hit.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     Whether or not the target entity died from this hit.
    /// </summary>
    public bool Kill { get; init; }

    /// <summary>
    ///     The amount of health restored to the source entity from lifesteal.
    /// </summary>
    public float LifeSteal { get; init; }

    /// <summary>
    ///     The amount of mana restored to the source entity from manasteal.
    /// </summary>
    public float ManaSteal { get; init; }

    /// <summary>
    ///     Whether or not the attack missed.
    ///     <br />
    ///     TODO: is there a default miss chance, separate from evasion?
    /// </summary>
    public bool Miss { get; init; }

    /// <summary>
    ///     If populated, the target's running combo counter - how many hits in a row it has taken.
    /// </summary>
    public int? Mobbing { get; init; }

    /// <summary>
    ///     If populated, the portion of this hit's damage absorbed by the target's mana shield, taken from their mana
    ///     instead of their health.
    /// </summary>
    [JsonProperty("mp_damage")]
    public float? MPDamage { get; init; }

    /// <summary>
    ///     If populated, the name of the projectile that caused this hit.
    /// </summary>
    public string? Projectile { get; init; }

    /// <summary>
    ///     If populated, the unique id of the projectile that caused this hit.
    /// </summary>
    [JsonProperty("pid")]
    public string? ProjectileId { get; init; }

    /// <summary>
    ///     The amount of damage returned to the attacker from this hit.
    ///     <br />
    ///     Reflect is for <see cref="AL.Core.Definitions.DamageType.Magical" /> damage only, and this number ignores the
    ///     source entity's resistance because this number results from calculating damage on the target entity using it's
    ///     resistance.
    /// </summary>
    public float Reflect { get; init; }

    /// <summary>
    ///     Whether or not this hit was from a rogue's sneak attack.
    /// </summary>
    public bool Sneak { get; init; }

    /// <summary>
    ///     The source of this hit. Generally a skill name.
    /// </summary>
    public string Source { get; init; } = null!;

    /// <summary>
    ///     Whether or not this hit was splash damage from an attack aimed at another entity.
    /// </summary>
    public bool? Splash { get; init; }

    /// <summary>
    ///     If populated, the ids of every entity stacked on the same tile as the target, all of which took this hit.
    /// </summary>
    public string[]? Stacked { get; init; }

    /// <summary>
    ///     Whether or not the target was stunned by this hit.
    /// </summary>
    public bool Stun { get; init; }
}