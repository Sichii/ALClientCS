#region
using Newtonsoft.Json;
#endregion

namespace AL.Data.Projectiles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class ProjectilesDatum : DatumBase<GProjectile>
    {
        [JsonProperty("acid")]
        public GProjectile Acid { get; init; } = null!;

        [JsonProperty("arrow")]
        public GProjectile Arrow { get; init; } = null!;

        [JsonProperty("bigmagic")]
        public GProjectile Bigmagic { get; init; } = null!;

        [JsonProperty("burst")]
        public GProjectile Burst { get; init; } = null!;

        [JsonProperty("crossbowarrow")]
        public GProjectile Crossbowarrow { get; init; } = null!;

        [JsonProperty("cupid")]
        public GProjectile Cupid { get; init; } = null!;

        [JsonProperty("curse")]
        public GProjectile Curse { get; init; } = null!;

        [JsonProperty("dartgun")]
        public GProjectile Dartgun { get; init; } = null!;

        [JsonProperty("firearrow")]
        public GProjectile Firearrow { get; init; } = null!;

        [JsonProperty("fireball")]
        public GProjectile Fireball { get; init; } = null!;

        [JsonProperty("frostarrow")]
        public GProjectile Frostarrow { get; init; } = null!;

        [JsonProperty("frostball")]
        public GProjectile Frostball { get; init; } = null!;

        [JsonProperty("garrow")]
        public GProjectile Garrow { get; init; } = null!;

        [JsonProperty("gburst")]
        public GProjectile Gburst { get; init; } = null!;

        [JsonProperty("magic")]
        public GProjectile Magic { get; init; } = null!;

        [JsonProperty("magic_divine")]
        public GProjectile MagicDivine { get; init; } = null!;

        [JsonProperty("magic_purple")]
        public GProjectile MagicPurple { get; init; } = null!;

        [JsonProperty("mentalburst")]
        public GProjectile Mentalburst { get; init; } = null!;

        [JsonProperty("mmagic")]
        public GProjectile Mmagic { get; init; } = null!;

        [JsonProperty("momentum")]
        public GProjectile Momentum { get; init; } = null!;

        [JsonProperty("partyheal")]
        public GProjectile Partyheal { get; init; } = null!;

        [JsonProperty("pinky")]
        public GProjectile Pinky { get; init; } = null!;

        [JsonProperty("plight")]
        public GProjectile Plight { get; init; } = null!;

        [JsonProperty("pmagic")]
        public GProjectile Pmagic { get; init; } = null!;

        [JsonProperty("poisonarrow")]
        public GProjectile Poisonarrow { get; init; } = null!;

        [JsonProperty("pouch")]
        public GProjectile Pouch { get; init; } = null!;

        [JsonProperty("purify")]
        public GProjectile Purify { get; init; } = null!;

        [JsonProperty("quickpunch")]
        public GProjectile Quickpunch { get; init; } = null!;

        [JsonProperty("quickstab")]
        public GProjectile Quickstab { get; init; } = null!;

        [JsonProperty("sburst")]
        public GProjectile Sburst { get; init; } = null!;

        [JsonProperty("smash")]
        public GProjectile Smash { get; init; } = null!;

        [JsonProperty("snowball")]
        public GProjectile Snowball { get; init; } = null!;

        [JsonProperty("stone")]
        public GProjectile Stone { get; init; } = null!;

        [JsonProperty("stone_k")]
        public GProjectile StoneK { get; init; } = null!;

        [JsonProperty("supershot")]
        public GProjectile Supershot { get; init; } = null!;

        [JsonProperty("wandy")]
        public GProjectile Wandy { get; init; } = null!;

        [JsonProperty("wmomentum")]
        public GProjectile Wmomentum { get; init; } = null!;
    }
}