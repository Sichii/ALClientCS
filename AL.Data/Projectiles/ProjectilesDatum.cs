using Newtonsoft.Json;

namespace AL.Data.Projectiles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ProjectilesDatum : DatumBase<GProjectile>
    {
        public GProjectile Acid { get; set; } = null!;
        public GProjectile Arrow { get; set; } = null!;
        public GProjectile BigMagic { get; set; } = null!;
        public GProjectile Burst { get; set; } = null!;
        public GProjectile CrossbowArrow { get; set; } = null!;
        public GProjectile Cupid { get; set; } = null!;
        public GProjectile Curse { get; set; } = null!;
        public GProjectile FireArrow { get; set; } = null!;
        public GProjectile Fireball { get; set; } = null!;
        public GProjectile FrostArrow { get; set; } = null!;
        public GProjectile FrostBall { get; set; } = null!;
        public GProjectile GArrow { get; set; } = null!;
        public GProjectile Magic { get; set; } = null!;

        [JsonProperty("magic_divine")]
        public GProjectile MagicDivine { get; set; } = null!;

        [JsonProperty("magic_purple")]
        public GProjectile MagicPurple { get; set; } = null!;

        public GProjectile MMagic { get; set; } = null!;
        public GProjectile Momentum { get; set; } = null!;
        public GProjectile Pinky { get; set; } = null!;
        public GProjectile Plight { get; set; } = null!;
        public GProjectile PMagic { get; set; } = null!;
        public GProjectile PoisonArrow { get; set; } = null!;
        public GProjectile Pouch { get; set; } = null!;
        public GProjectile SnowBall { get; set; } = null!;
        public GProjectile Stone { get; set; } = null!;

        [JsonProperty("stone_k")]
        public GProjectile StoneK { get; set; } = null!;

        public GProjectile SuperShot { get; set; } = null!;
        public GProjectile Wandy { get; set; } = null!;
        public GProjectile WMomentum { get; set; } = null!;
    }
}