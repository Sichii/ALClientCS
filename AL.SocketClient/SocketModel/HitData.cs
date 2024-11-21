#region
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when an entity is hit.
/// </summary>
public sealed record HitData
{
    /// <summary>
    ///     GUI related. If populated, name of the animation played for this hit.
    /// </summary>
    [JsonProperty("anim")]
    public string? Animation { get; init; }

    /// <summary>
    ///     TODO: Unsure, I see it a lot for heal projectiles when the target moves a long distance away from their initial
    ///     location (so they avoided the projectile?), but the target still gets healed.
    /// </summary>
    public bool Avoid { get; init; }

    /// <summary>
    ///     The amount of damage this hit did.
    /// </summary>
    public int Damage { get; init; }

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
    ///     A unique identifier for the hit.
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
    ///     Whether or not the target was stunned by this hit.
    /// </summary>
    public bool Stun { get; init; }
}