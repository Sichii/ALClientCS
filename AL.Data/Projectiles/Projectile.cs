﻿namespace AL.Data.Projectiles
{
    public record Projectile
    {
        /// <summary>
        ///     TODO: Unknown, something to do with snowballs.
        /// </summary>
        public bool Pure { get; init; }

        /// <summary>
        ///     Whether or not this projectile is actually a ray.
        /// </summary>
        public bool Ray { get; init; }

        /// <summary>
        ///     The speed the projectile moves at.
        /// </summary>
        public float Speed { get; init; }
    }
}