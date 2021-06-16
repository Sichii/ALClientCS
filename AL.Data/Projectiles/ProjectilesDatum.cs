using Newtonsoft.Json;

namespace AL.Data.Projectiles
{
    public class ProjectilesDatum : DatumBase<Projectile>
    {
        public Projectile Acid { get; set; }
        public Projectile Arrow { get; set; }
        public Projectile BigMagic { get; set; }
        public Projectile Burst { get; set; }
        public Projectile CrossbowArrow { get; set; }
        public Projectile Cupid { get; set; }
        public Projectile Curse { get; set; }
        public Projectile FireArrow { get; set; }
        public Projectile Fireball { get; set; }
        public Projectile FrostArrow { get; set; }
        public Projectile FrostBall { get; set; }
        public Projectile GArrow { get; set; }
        public Projectile Magic { get; set; }

        [JsonProperty("magic_divine")]
        public Projectile MagicDivine { get; set; }

        [JsonProperty("magic_purple")]
        public Projectile MagicPurple { get; set; }

        public Projectile MMagic { get; set; }
        public Projectile Momentum { get; set; }
        public Projectile Pinky { get; set; }
        public Projectile Plight { get; set; }
        public Projectile PMagic { get; set; }
        public Projectile PoisonArrow { get; set; }
        public Projectile Pouch { get; set; }
        public Projectile SnowBall { get; set; }
        public Projectile Stone { get; set; }

        [JsonProperty("stone_k")]
        public Projectile StoneK { get; set; }

        public Projectile SuperShot { get; set; }
        public Projectile Wandy { get; set; }
        public Projectile WMomentum { get; set; }
    }
}