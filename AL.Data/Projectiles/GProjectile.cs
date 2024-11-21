#region
using AL.Core.Definitions;
#endregion

namespace AL.Data.Projectiles
{
    public record GProjectile
    {
        /// <summary>
        ///     TODO: Unknown, something to do with snowballs.
        /// </summary>
        public bool Pure { get; init; }

        /// <summary>
        ///     Whether or not this projectile is actually a ray.
        /// </summary>
        public RayType Ray { get; init; }

        /// <summary>
        ///     The speed the projectile moves at.
        /// </summary>
        public float Speed { get; init; }
    }
}