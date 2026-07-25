#region
using System.Text.Json.Serialization;
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
        [JsonPropertyName("acid")]
        public GProjectile Acid { get; init; } = null!;

        [JsonProperty("arrow")]
        [JsonPropertyName("arrow")]
        public GProjectile Arrow { get; init; } = null!;

        [JsonProperty("bigmagic")]
        [JsonPropertyName("bigmagic")]
        public GProjectile Bigmagic { get; init; } = null!;

        [JsonProperty("burst")]
        [JsonPropertyName("burst")]
        public GProjectile Burst { get; init; } = null!;

        [JsonProperty("crossbowarrow")]
        [JsonPropertyName("crossbowarrow")]
        public GProjectile Crossbowarrow { get; init; } = null!;

        [JsonProperty("cupid")]
        [JsonPropertyName("cupid")]
        public GProjectile Cupid { get; init; } = null!;

        [JsonProperty("curse")]
        [JsonPropertyName("curse")]
        public GProjectile Curse { get; init; } = null!;

        [JsonProperty("dartgun")]
        [JsonPropertyName("dartgun")]
        public GProjectile Dartgun { get; init; } = null!;

        [JsonProperty("firearrow")]
        [JsonPropertyName("firearrow")]
        public GProjectile Firearrow { get; init; } = null!;

        [JsonProperty("fireball")]
        [JsonPropertyName("fireball")]
        public GProjectile Fireball { get; init; } = null!;

        [JsonProperty("frostarrow")]
        [JsonPropertyName("frostarrow")]
        public GProjectile Frostarrow { get; init; } = null!;

        [JsonProperty("frostball")]
        [JsonPropertyName("frostball")]
        public GProjectile Frostball { get; init; } = null!;

        [JsonProperty("garrow")]
        [JsonPropertyName("garrow")]
        public GProjectile Garrow { get; init; } = null!;

        [JsonProperty("gburst")]
        [JsonPropertyName("gburst")]
        public GProjectile Gburst { get; init; } = null!;

        [JsonProperty("magic")]
        [JsonPropertyName("magic")]
        public GProjectile Magic { get; init; } = null!;

        [JsonProperty("magic_divine")]
        [JsonPropertyName("magic_divine")]
        public GProjectile MagicDivine { get; init; } = null!;

        [JsonProperty("magic_purple")]
        [JsonPropertyName("magic_purple")]
        public GProjectile MagicPurple { get; init; } = null!;

        [JsonProperty("mentalburst")]
        [JsonPropertyName("mentalburst")]
        public GProjectile Mentalburst { get; init; } = null!;

        [JsonProperty("mmagic")]
        [JsonPropertyName("mmagic")]
        public GProjectile Mmagic { get; init; } = null!;

        [JsonProperty("momentum")]
        [JsonPropertyName("momentum")]
        public GProjectile Momentum { get; init; } = null!;

        [JsonProperty("partyheal")]
        [JsonPropertyName("partyheal")]
        public GProjectile Partyheal { get; init; } = null!;

        [JsonProperty("pinky")]
        [JsonPropertyName("pinky")]
        public GProjectile Pinky { get; init; } = null!;

        [JsonProperty("plight")]
        [JsonPropertyName("plight")]
        public GProjectile Plight { get; init; } = null!;

        [JsonProperty("pmagic")]
        [JsonPropertyName("pmagic")]
        public GProjectile Pmagic { get; init; } = null!;

        [JsonProperty("poisonarrow")]
        [JsonPropertyName("poisonarrow")]
        public GProjectile Poisonarrow { get; init; } = null!;

        [JsonProperty("pouch")]
        [JsonPropertyName("pouch")]
        public GProjectile Pouch { get; init; } = null!;

        [JsonProperty("purify")]
        [JsonPropertyName("purify")]
        public GProjectile Purify { get; init; } = null!;

        [JsonProperty("quickpunch")]
        [JsonPropertyName("quickpunch")]
        public GProjectile Quickpunch { get; init; } = null!;

        [JsonProperty("quickstab")]
        [JsonPropertyName("quickstab")]
        public GProjectile Quickstab { get; init; } = null!;

        [JsonProperty("sburst")]
        [JsonPropertyName("sburst")]
        public GProjectile Sburst { get; init; } = null!;

        [JsonProperty("smash")]
        [JsonPropertyName("smash")]
        public GProjectile Smash { get; init; } = null!;

        [JsonProperty("snowball")]
        [JsonPropertyName("snowball")]
        public GProjectile Snowball { get; init; } = null!;

        [JsonProperty("stone")]
        [JsonPropertyName("stone")]
        public GProjectile Stone { get; init; } = null!;

        [JsonProperty("stone_k")]
        [JsonPropertyName("stone_k")]
        public GProjectile StoneK { get; init; } = null!;

        [JsonProperty("supershot")]
        [JsonPropertyName("supershot")]
        public GProjectile Supershot { get; init; } = null!;

        [JsonProperty("wandy")]
        [JsonPropertyName("wandy")]
        public GProjectile Wandy { get; init; } = null!;

        [JsonProperty("wmomentum")]
        [JsonPropertyName("wmomentum")]
        public GProjectile Wmomentum { get; init; } = null!;
    }
}