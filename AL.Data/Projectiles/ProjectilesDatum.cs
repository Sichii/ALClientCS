#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Projectiles;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class ProjectilesDatum : DatumBase<GProjectile>
{
    [JsonPropertyName("acid")]
    public GProjectile Acid { get; init; } = null!;

    [JsonPropertyName("arrow")]
    public GProjectile Arrow { get; init; } = null!;

    [JsonPropertyName("bigmagic")]
    public GProjectile Bigmagic { get; init; } = null!;

    [JsonPropertyName("burst")]
    public GProjectile Burst { get; init; } = null!;

    [JsonPropertyName("crossbowarrow")]
    public GProjectile Crossbowarrow { get; init; } = null!;

    [JsonPropertyName("cupid")]
    public GProjectile Cupid { get; init; } = null!;

    [JsonPropertyName("curse")]
    public GProjectile Curse { get; init; } = null!;

    [JsonPropertyName("dartgun")]
    public GProjectile Dartgun { get; init; } = null!;

    [JsonPropertyName("firearrow")]
    public GProjectile Firearrow { get; init; } = null!;

    [JsonPropertyName("fireball")]
    public GProjectile Fireball { get; init; } = null!;

    [JsonPropertyName("frostarrow")]
    public GProjectile Frostarrow { get; init; } = null!;

    [JsonPropertyName("frostball")]
    public GProjectile Frostball { get; init; } = null!;

    [JsonPropertyName("garrow")]
    public GProjectile Garrow { get; init; } = null!;

    [JsonPropertyName("gburst")]
    public GProjectile Gburst { get; init; } = null!;

    [JsonPropertyName("magic")]
    public GProjectile Magic { get; init; } = null!;

    [JsonPropertyName("magic_divine")]
    public GProjectile MagicDivine { get; init; } = null!;

    [JsonPropertyName("magic_purple")]
    public GProjectile MagicPurple { get; init; } = null!;

    [JsonPropertyName("mentalburst")]
    public GProjectile Mentalburst { get; init; } = null!;

    [JsonPropertyName("mmagic")]
    public GProjectile Mmagic { get; init; } = null!;

    [JsonPropertyName("momentum")]
    public GProjectile Momentum { get; init; } = null!;

    [JsonPropertyName("partyheal")]
    public GProjectile Partyheal { get; init; } = null!;

    [JsonPropertyName("pinky")]
    public GProjectile Pinky { get; init; } = null!;

    [JsonPropertyName("plight")]
    public GProjectile Plight { get; init; } = null!;

    [JsonPropertyName("pmagic")]
    public GProjectile Pmagic { get; init; } = null!;

    [JsonPropertyName("poisonarrow")]
    public GProjectile Poisonarrow { get; init; } = null!;

    [JsonPropertyName("pouch")]
    public GProjectile Pouch { get; init; } = null!;

    [JsonPropertyName("purify")]
    public GProjectile Purify { get; init; } = null!;

    [JsonPropertyName("quickpunch")]
    public GProjectile Quickpunch { get; init; } = null!;

    [JsonPropertyName("quickstab")]
    public GProjectile Quickstab { get; init; } = null!;

    [JsonPropertyName("sburst")]
    public GProjectile Sburst { get; init; } = null!;

    [JsonPropertyName("smash")]
    public GProjectile Smash { get; init; } = null!;

    [JsonPropertyName("snowball")]
    public GProjectile Snowball { get; init; } = null!;

    [JsonPropertyName("stone")]
    public GProjectile Stone { get; init; } = null!;

    [JsonPropertyName("stone_k")]
    public GProjectile StoneK { get; init; } = null!;

    [JsonPropertyName("supershot")]
    public GProjectile Supershot { get; init; } = null!;

    [JsonPropertyName("wandy")]
    public GProjectile Wandy { get; init; } = null!;

    [JsonPropertyName("wmomentum")]
    public GProjectile Wmomentum { get; init; } = null!;
}