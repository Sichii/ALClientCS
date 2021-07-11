using Newtonsoft.Json;

namespace AL.Data.Projectiles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ProjectilesDatum : DatumBase<Projectile>
    {
        public Projectile Acid { get; set; } = null!;
        public Projectile Arrow { get; set; } = null!;
        public Projectile BigMagic { get; set; } = null!;
        public Projectile Burst { get; set; } = null!;
        public Projectile CrossbowArrow { get; set; } = null!;
        public Projectile Cupid { get; set; } = null!;
        public Projectile Curse { get; set; } = null!;
        public Projectile FireArrow { get; set; } = null!;
        public Projectile Fireball { get; set; } = null!;
        public Projectile FrostArrow { get; set; } = null!;
        public Projectile FrostBall { get; set; } = null!;
        public Projectile GArrow { get; set; } = null!;
        public Projectile Magic { get; set; } = null!;

        [JsonProperty("magic_divine")]
        public Projectile MagicDivine { get; set; } = null!;

        [JsonProperty("magic_purple")]
        public Projectile MagicPurple { get; set; } = null!;

        public Projectile MMagic { get; set; } = null!;
        public Projectile Momentum { get; set; } = null!;
        public Projectile Pinky { get; set; } = null!;
        public Projectile Plight { get; set; } = null!;
        public Projectile PMagic { get; set; } = null!;
        public Projectile PoisonArrow { get; set; } = null!;
        public Projectile Pouch { get; set; } = null!;
        public Projectile SnowBall { get; set; } = null!;
        public Projectile Stone { get; set; } = null!;

        [JsonProperty("stone_k")]
        public Projectile StoneK { get; set; } = null!;

        public Projectile SuperShot { get; set; } = null!;
        public Projectile Wandy { get; set; } = null!;
        public Projectile WMomentum { get; set; } = null!;
    }
}