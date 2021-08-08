using Newtonsoft.Json;

namespace AL.Data.Projectiles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ProjectilesDatum : DatumBase<GProjectile>
    {
        public GProjectile Acid { get; init; } = null!;
        public GProjectile Arrow { get; init; } = null!;
        public GProjectile Bigmagic { get; init; } = null!;
        public GProjectile Burst { get; init; } = null!;
        public GProjectile Crossbowarrow { get; init; } = null!;
        public GProjectile Cupid { get; init; } = null!;
        public GProjectile Curse { get; init; } = null!;
        public GProjectile Firearrow { get; init; } = null!;
        public GProjectile Fireball { get; init; } = null!;
        public GProjectile Frostarrow { get; init; } = null!;
        public GProjectile Frostball { get; init; } = null!;
        public GProjectile Garrow { get; init; } = null!;
        public GProjectile Magic { get; init; } = null!;
        [JsonProperty("magic_divine")]
        public GProjectile MagicDivine { get; init; } = null!;
        [JsonProperty("magic_purple")]
        public GProjectile MagicPurple { get; init; } = null!;
        public GProjectile Mmagic { get; init; } = null!;
        public GProjectile Momentum { get; init; } = null!;
        public GProjectile Pinky { get; init; } = null!;
        public GProjectile Plight { get; init; } = null!;
        public GProjectile Pmagic { get; init; } = null!;
        public GProjectile Poisonarrow { get; init; } = null!;
        public GProjectile Pouch { get; init; } = null!;
        public GProjectile Snowball { get; init; } = null!;
        public GProjectile Stone { get; init; } = null!;
        [JsonProperty("stone_k")]
        public GProjectile StoneK { get; init; } = null!;
        public GProjectile Supershot { get; init; } = null!;
        public GProjectile Wandy { get; init; } = null!;
        public GProjectile Wmomentum { get; init; } = null!;
    }
}