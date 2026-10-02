#region
using AL.Core.Definitions;
#endregion

namespace AL.Data.Projectiles;

public record GProjectile
{
    /// <summary>
    ///     Cosmetic only: the sprite is drawn without the attacker's weapon glow tint. Only the snowball carries it.
    /// </summary>
    public bool Pure { get; init; }

    /// <summary>
    ///     If populated, the projectile is drawn as a continuous beam between attacker and target using this tiling animation,
    ///     instead of a sprite that travels.
    /// </summary>
    public RayType Ray { get; init; }

    /// <summary>
    ///     The speed the projectile moves at, in units per second. It sets the flight time before the hit lands: the
    ///     edge-to-edge distance divided by this.
    /// </summary>
    public float Speed { get; init; }
}