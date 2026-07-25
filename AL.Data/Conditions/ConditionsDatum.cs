#region
using System.Text.Json.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Conditions
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GCondition>))]
    public class ConditionsDatum : DatumBase<GCondition>
    {
        [JsonProperty("authfail")]
        [JsonPropertyName("authfail")]
        public GCondition Authfail { get; init; } = null!;

        [JsonProperty("blink")]
        [JsonPropertyName("blink")]
        public GCondition Blink { get; init; } = null!;

        [JsonProperty("block")]
        [JsonPropertyName("block")]
        public GCondition Block { get; init; } = null!;

        [JsonProperty("burned")]
        [JsonPropertyName("burned")]
        public GCondition Burned { get; init; } = null!;

        [JsonProperty("charging")]
        [JsonPropertyName("charging")]
        public GCondition Charging { get; init; } = null!;

        [JsonProperty("charmed")]
        [JsonPropertyName("charmed")]
        public GCondition Charmed { get; init; } = null!;

        [JsonProperty("cursed")]
        [JsonPropertyName("cursed")]
        public GCondition Cursed { get; init; } = null!;

        [JsonProperty("dampened")]
        [JsonPropertyName("dampened")]
        public GCondition Dampened { get; init; } = null!;

        [JsonProperty("darkblessing")]
        [JsonPropertyName("darkblessing")]
        public GCondition Darkblessing { get; init; } = null!;

        [JsonProperty("dash")]
        [JsonPropertyName("dash")]
        public GCondition Dash { get; init; } = null!;

        [JsonProperty("deepfreezed")]
        [JsonPropertyName("deepfreezed")]
        public GCondition Deepfreezed { get; init; } = null!;

        [JsonProperty("easterluck")]
        [JsonPropertyName("easterluck")]
        public GCondition Easterluck { get; init; } = null!;

        [JsonProperty("eburn")]
        [JsonPropertyName("eburn")]
        public GCondition Eburn { get; init; } = null!;

        [JsonProperty("eheal")]
        [JsonPropertyName("eheal")]
        public GCondition Eheal { get; init; } = null!;

        [JsonProperty("energized")]
        [JsonPropertyName("energized")]
        public GCondition Energized { get; init; } = null!;

        [JsonProperty("fingered")]
        [JsonPropertyName("fingered")]
        public GCondition Fingered { get; init; } = null!;

        [JsonProperty("fishing")]
        [JsonPropertyName("fishing")]
        public GCondition Fishing { get; init; } = null!;

        [JsonProperty("frozen")]
        [JsonPropertyName("frozen")]
        public GCondition Frozen { get; init; } = null!;

        [JsonProperty("fullguard")]
        [JsonPropertyName("fullguard")]
        public GCondition Fullguard { get; init; } = null!;

        [JsonProperty("fullguardx")]
        [JsonPropertyName("fullguardx")]
        public GCondition Fullguardx { get; init; } = null!;

        [JsonProperty("halloween0")]
        [JsonPropertyName("halloween0")]
        public GCondition Halloween0 { get; init; } = null!;

        [JsonProperty("halloween1")]
        [JsonPropertyName("halloween1")]
        public GCondition Halloween1 { get; init; } = null!;

        [JsonProperty("halloween2")]
        [JsonPropertyName("halloween2")]
        public GCondition Halloween2 { get; init; } = null!;

        [JsonProperty("hardshell")]
        [JsonPropertyName("hardshell")]
        public GCondition Hardshell { get; init; } = null!;

        [JsonProperty("holidayspirit")]
        [JsonPropertyName("holidayspirit")]
        public GCondition Holidayspirit { get; init; } = null!;

        [JsonProperty("hopsickness")]
        [JsonPropertyName("hopsickness")]
        public GCondition Hopsickness { get; init; } = null!;

        [JsonProperty("invincible")]
        [JsonPropertyName("invincible")]
        public GCondition Invincible { get; init; } = null!;

        [JsonProperty("invis")]
        [JsonPropertyName("invis")]
        public GCondition Invis { get; init; } = null!;

        [JsonProperty("licenced")]
        [JsonPropertyName("licenced")]
        public GCondition Licensed { get; init; } = null!;

        [JsonProperty("marked")]
        [JsonPropertyName("marked")]
        public GCondition Marked { get; init; } = null!;

        [JsonProperty("massexchange")]
        [JsonPropertyName("massexchange")]
        public GCondition Massexchange { get; init; } = null!;

        [JsonProperty("massexchangepp")]
        [JsonPropertyName("massexchangepp")]
        public GCondition Massexchangepp { get; init; } = null!;

        [JsonProperty("massproduction")]
        [JsonPropertyName("massproduction")]
        public GCondition Massproduction { get; init; } = null!;

        [JsonProperty("massproductionpp")]
        [JsonPropertyName("massproductionpp")]
        public GCondition Massproductionpp { get; init; } = null!;

        [JsonProperty("mcourage")]
        [JsonPropertyName("mcourage")]
        public GCondition Mcourage { get; init; } = null!;

        [JsonProperty("mfrenzy")]
        [JsonPropertyName("mfrenzy")]
        public GCondition Mfrenzy { get; init; } = null!;

        [JsonProperty("mining")]
        [JsonPropertyName("mining")]
        public GCondition Mining { get; init; } = null!;

        [JsonProperty("mlifesteal")]
        [JsonPropertyName("mlifesteal")]
        public GCondition Mlifesteal { get; init; } = null!;

        [JsonProperty("mluck")]
        [JsonPropertyName("mluck")]
        public GCondition Mluck { get; init; } = null!;

        [JsonProperty("monsterhunt")]
        [JsonPropertyName("monsterhunt")]
        public GCondition Monsterhunt { get; init; } = null!;

        [JsonProperty("mshield")]
        [JsonPropertyName("mshield")]
        public GCondition Mshield { get; init; } = null!;

        [JsonProperty("newcomersblessing")]
        [JsonPropertyName("newcomersblessing")]
        public GCondition Newcomersblessing { get; init; } = null!;

        [JsonProperty("notverified")]
        [JsonPropertyName("notverified")]
        public GCondition Notverified { get; init; } = null!;

        [JsonProperty("patronsgrace")]
        [JsonPropertyName("patronsgrace")]
        public GCondition Patronsgrace { get; init; } = null!;

        [JsonProperty("penalty_cd")]
        [JsonPropertyName("penalty_cd")]
        public GCondition PenaltyCd { get; init; } = null!;

        [JsonProperty("phasedout")]
        [JsonPropertyName("phasedout")]
        public GCondition Phasedout { get; init; } = null!;

        [JsonProperty("pickpocket")]
        [JsonPropertyName("pickpocket")]
        public GCondition Pickpocket { get; init; } = null!;

        [JsonProperty("poisoned")]
        [JsonPropertyName("poisoned")]
        public GCondition Poisoned { get; init; } = null!;

        [JsonProperty("poisonous")]
        [JsonPropertyName("poisonous")]
        public GCondition Poisonous { get; init; } = null!;

        [JsonProperty("power")]
        [JsonPropertyName("power")]
        public GCondition Power { get; init; } = null!;

        [JsonProperty("purifier")]
        [JsonPropertyName("purifier")]
        public GCondition Purifier { get; init; } = null!;

        [JsonProperty("reflection")]
        [JsonPropertyName("reflection")]
        public GCondition Reflection { get; init; } = null!;

        [JsonProperty("rspeed")]
        [JsonPropertyName("rspeed")]
        public GCondition Rspeed { get; init; } = null!;

        [JsonProperty("sanguine")]
        [JsonPropertyName("sanguine")]
        public GCondition Sanguine { get; init; } = null!;

        [JsonProperty("shocked")]
        [JsonPropertyName("shocked")]
        public GCondition Shocked { get; init; } = null!;

        [JsonProperty("sleeping")]
        [JsonPropertyName("sleeping")]
        public GCondition Sleeping { get; init; } = null!;

        [JsonProperty("slowness")]
        [JsonPropertyName("slowness")]
        public GCondition Slowness { get; init; } = null!;

        [JsonProperty("stack")]
        [JsonPropertyName("stack")]
        public GCondition Stack { get; init; } = null!;

        [JsonProperty("stoned")]
        [JsonPropertyName("stoned")]
        public GCondition Stoned { get; init; } = null!;

        [JsonProperty("stunned")]
        [JsonPropertyName("stunned")]
        public GCondition Stunned { get; init; } = null!;

        [JsonProperty("sugarrush")]
        [JsonPropertyName("sugarrush")]
        public GCondition Sugarrush { get; init; } = null!;

        [JsonProperty("tangled")]
        [JsonPropertyName("tangled")]
        public GCondition Tangled { get; init; } = null!;

        [JsonProperty("town")]
        [JsonPropertyName("town")]
        public GCondition Town { get; init; } = null!;

        [JsonProperty("warcry")]
        [JsonPropertyName("warcry")]
        public GCondition Warcry { get; init; } = null!;

        [JsonProperty("weakness")]
        [JsonPropertyName("weakness")]
        public GCondition Weakness { get; init; } = null!;

        [JsonProperty("withdrawal")]
        [JsonPropertyName("withdrawal")]
        public GCondition Withdrawal { get; init; } = null!;

        [JsonProperty("woven")]
        [JsonPropertyName("woven")]
        public GCondition Woven { get; init; } = null!;

        [JsonProperty("xpower")]
        [JsonPropertyName("xpower")]
        public GCondition Xpower { get; init; } = null!;

        [JsonProperty("xshotted")]
        [JsonPropertyName("xshotted")]
        public GCondition Xshotted { get; init; } = null!;
    }
}