#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Conditions;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class ConditionsDatum : DatumBase<GCondition>
{
    [JsonPropertyName("authfail")]
    public GCondition Authfail { get; init; } = null!;

    [JsonPropertyName("blink")]
    public GCondition Blink { get; init; } = null!;

    [JsonPropertyName("block")]
    public GCondition Block { get; init; } = null!;

    [JsonPropertyName("burned")]
    public GCondition Burned { get; init; } = null!;

    [JsonPropertyName("charging")]
    public GCondition Charging { get; init; } = null!;

    [JsonPropertyName("charmed")]
    public GCondition Charmed { get; init; } = null!;

    [JsonPropertyName("cursed")]
    public GCondition Cursed { get; init; } = null!;

    [JsonPropertyName("dampened")]
    public GCondition Dampened { get; init; } = null!;

    [JsonPropertyName("darkblessing")]
    public GCondition Darkblessing { get; init; } = null!;

    [JsonPropertyName("dash")]
    public GCondition Dash { get; init; } = null!;

    [JsonPropertyName("deepfreezed")]
    public GCondition Deepfreezed { get; init; } = null!;

    [JsonPropertyName("easterluck")]
    public GCondition Easterluck { get; init; } = null!;

    [JsonPropertyName("eburn")]
    public GCondition Eburn { get; init; } = null!;

    [JsonPropertyName("eheal")]
    public GCondition Eheal { get; init; } = null!;

    [JsonPropertyName("energized")]
    public GCondition Energized { get; init; } = null!;

    [JsonPropertyName("fingered")]
    public GCondition Fingered { get; init; } = null!;

    [JsonPropertyName("fishing")]
    public GCondition Fishing { get; init; } = null!;

    [JsonPropertyName("frozen")]
    public GCondition Frozen { get; init; } = null!;

    [JsonPropertyName("fullguard")]
    public GCondition Fullguard { get; init; } = null!;

    [JsonPropertyName("fullguardx")]
    public GCondition Fullguardx { get; init; } = null!;

    [JsonPropertyName("halloween0")]
    public GCondition Halloween0 { get; init; } = null!;

    [JsonPropertyName("halloween1")]
    public GCondition Halloween1 { get; init; } = null!;

    [JsonPropertyName("halloween2")]
    public GCondition Halloween2 { get; init; } = null!;

    [JsonPropertyName("hardshell")]
    public GCondition Hardshell { get; init; } = null!;

    [JsonPropertyName("holidayspirit")]
    public GCondition Holidayspirit { get; init; } = null!;

    [JsonPropertyName("hopsickness")]
    public GCondition Hopsickness { get; init; } = null!;

    [JsonPropertyName("invincible")]
    public GCondition Invincible { get; init; } = null!;

    [JsonPropertyName("invis")]
    public GCondition Invis { get; init; } = null!;

    [JsonPropertyName("licenced")]
    public GCondition Licensed { get; init; } = null!;

    [JsonPropertyName("marked")]
    public GCondition Marked { get; init; } = null!;

    [JsonPropertyName("massexchange")]
    public GCondition Massexchange { get; init; } = null!;

    [JsonPropertyName("massexchangepp")]
    public GCondition Massexchangepp { get; init; } = null!;

    [JsonPropertyName("massproduction")]
    public GCondition Massproduction { get; init; } = null!;

    [JsonPropertyName("massproductionpp")]
    public GCondition Massproductionpp { get; init; } = null!;

    [JsonPropertyName("mcourage")]
    public GCondition Mcourage { get; init; } = null!;

    [JsonPropertyName("mfrenzy")]
    public GCondition Mfrenzy { get; init; } = null!;

    [JsonPropertyName("mining")]
    public GCondition Mining { get; init; } = null!;

    [JsonPropertyName("mlifesteal")]
    public GCondition Mlifesteal { get; init; } = null!;

    [JsonPropertyName("mluck")]
    public GCondition Mluck { get; init; } = null!;

    [JsonPropertyName("monsterhunt")]
    public GCondition Monsterhunt { get; init; } = null!;

    [JsonPropertyName("mshield")]
    public GCondition Mshield { get; init; } = null!;

    [JsonPropertyName("newcomersblessing")]
    public GCondition Newcomersblessing { get; init; } = null!;

    [JsonPropertyName("notverified")]
    public GCondition Notverified { get; init; } = null!;

    [JsonPropertyName("patronsgrace")]
    public GCondition Patronsgrace { get; init; } = null!;

    [JsonPropertyName("penalty_cd")]
    public GCondition PenaltyCd { get; init; } = null!;

    [JsonPropertyName("phasedout")]
    public GCondition Phasedout { get; init; } = null!;

    [JsonPropertyName("pickpocket")]
    public GCondition Pickpocket { get; init; } = null!;

    [JsonPropertyName("poisoned")]
    public GCondition Poisoned { get; init; } = null!;

    [JsonPropertyName("poisonous")]
    public GCondition Poisonous { get; init; } = null!;

    [JsonPropertyName("power")]
    public GCondition Power { get; init; } = null!;

    [JsonPropertyName("purifier")]
    public GCondition Purifier { get; init; } = null!;

    [JsonPropertyName("reflection")]
    public GCondition Reflection { get; init; } = null!;

    [JsonPropertyName("rspeed")]
    public GCondition Rspeed { get; init; } = null!;

    [JsonPropertyName("sanguine")]
    public GCondition Sanguine { get; init; } = null!;

    [JsonPropertyName("shocked")]
    public GCondition Shocked { get; init; } = null!;

    [JsonPropertyName("sleeping")]
    public GCondition Sleeping { get; init; } = null!;

    [JsonPropertyName("slowness")]
    public GCondition Slowness { get; init; } = null!;

    [JsonPropertyName("stack")]
    public GCondition Stack { get; init; } = null!;

    [JsonPropertyName("stoned")]
    public GCondition Stoned { get; init; } = null!;

    [JsonPropertyName("stunned")]
    public GCondition Stunned { get; init; } = null!;

    [JsonPropertyName("sugarrush")]
    public GCondition Sugarrush { get; init; } = null!;

    [JsonPropertyName("tangled")]
    public GCondition Tangled { get; init; } = null!;

    [JsonPropertyName("town")]
    public GCondition Town { get; init; } = null!;

    [JsonPropertyName("warcry")]
    public GCondition Warcry { get; init; } = null!;

    [JsonPropertyName("weakness")]
    public GCondition Weakness { get; init; } = null!;

    [JsonPropertyName("withdrawal")]
    public GCondition Withdrawal { get; init; } = null!;

    [JsonPropertyName("woven")]
    public GCondition Woven { get; init; } = null!;

    [JsonPropertyName("xpower")]
    public GCondition Xpower { get; init; } = null!;

    [JsonPropertyName("xshotted")]
    public GCondition Xshotted { get; init; } = null!;
}