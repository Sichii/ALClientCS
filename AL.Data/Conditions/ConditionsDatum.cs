#region
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
        public GCondition Authfail { get; init; } = null!;

        [JsonProperty("blink")]
        public GCondition Blink { get; init; } = null!;

        [JsonProperty("block")]
        public GCondition Block { get; init; } = null!;

        [JsonProperty("burned")]
        public GCondition Burned { get; init; } = null!;

        [JsonProperty("charging")]
        public GCondition Charging { get; init; } = null!;

        [JsonProperty("charmed")]
        public GCondition Charmed { get; init; } = null!;

        [JsonProperty("cursed")]
        public GCondition Cursed { get; init; } = null!;

        [JsonProperty("dampened")]
        public GCondition Dampened { get; init; } = null!;

        [JsonProperty("darkblessing")]
        public GCondition Darkblessing { get; init; } = null!;

        [JsonProperty("dash")]
        public GCondition Dash { get; init; } = null!;

        [JsonProperty("deepfreezed")]
        public GCondition Deepfreezed { get; init; } = null!;

        [JsonProperty("easterluck")]
        public GCondition Easterluck { get; init; } = null!;

        [JsonProperty("eburn")]
        public GCondition Eburn { get; init; } = null!;

        [JsonProperty("eheal")]
        public GCondition Eheal { get; init; } = null!;

        [JsonProperty("energized")]
        public GCondition Energized { get; init; } = null!;

        [JsonProperty("fingered")]
        public GCondition Fingered { get; init; } = null!;

        [JsonProperty("fishing")]
        public GCondition Fishing { get; init; } = null!;

        [JsonProperty("frozen")]
        public GCondition Frozen { get; init; } = null!;

        [JsonProperty("fullguard")]
        public GCondition Fullguard { get; init; } = null!;

        [JsonProperty("fullguardx")]
        public GCondition Fullguardx { get; init; } = null!;

        [JsonProperty("halloween0")]
        public GCondition Halloween0 { get; init; } = null!;

        [JsonProperty("halloween1")]
        public GCondition Halloween1 { get; init; } = null!;

        [JsonProperty("halloween2")]
        public GCondition Halloween2 { get; init; } = null!;

        [JsonProperty("hardshell")]
        public GCondition Hardshell { get; init; } = null!;

        [JsonProperty("holidayspirit")]
        public GCondition Holidayspirit { get; init; } = null!;

        [JsonProperty("hopsickness")]
        public GCondition Hopsickness { get; init; } = null!;

        [JsonProperty("invincible")]
        public GCondition Invincible { get; init; } = null!;

        [JsonProperty("invis")]
        public GCondition Invis { get; init; } = null!;

        [JsonProperty("licenced")]
        public GCondition Licensed { get; init; } = null!;

        [JsonProperty("marked")]
        public GCondition Marked { get; init; } = null!;

        [JsonProperty("massproduction")]
        public GCondition Massproduction { get; init; } = null!;

        [JsonProperty("massproductionpp")]
        public GCondition Massproductionpp { get; init; } = null!;

        [JsonProperty("mcourage")]
        public GCondition Mcourage { get; init; } = null!;

        [JsonProperty("mfrenzy")]
        public GCondition Mfrenzy { get; init; } = null!;

        [JsonProperty("mining")]
        public GCondition Mining { get; init; } = null!;

        [JsonProperty("mlifesteal")]
        public GCondition Mlifesteal { get; init; } = null!;

        [JsonProperty("mluck")]
        public GCondition Mluck { get; init; } = null!;

        [JsonProperty("monsterhunt")]
        public GCondition Monsterhunt { get; init; } = null!;

        [JsonProperty("mshield")]
        public GCondition Mshield { get; init; } = null!;

        [JsonProperty("newcomersblessing")]
        public GCondition Newcomersblessing { get; init; } = null!;

        [JsonProperty("notverified")]
        public GCondition Notverified { get; init; } = null!;

        [JsonProperty("penalty_cd")]
        public GCondition PenaltyCd { get; init; } = null!;

        [JsonProperty("phasedout")]
        public GCondition Phasedout { get; init; } = null!;

        [JsonProperty("pickpocket")]
        public GCondition Pickpocket { get; init; } = null!;

        [JsonProperty("poisoned")]
        public GCondition Poisoned { get; init; } = null!;

        [JsonProperty("poisonous")]
        public GCondition Poisonous { get; init; } = null!;

        [JsonProperty("power")]
        public GCondition Power { get; init; } = null!;

        [JsonProperty("purifier")]
        public GCondition Purifier { get; init; } = null!;

        [JsonProperty("reflection")]
        public GCondition Reflection { get; init; } = null!;

        [JsonProperty("rspeed")]
        public GCondition Rspeed { get; init; } = null!;

        [JsonProperty("sanguine")]
        public GCondition Sanguine { get; init; } = null!;

        [JsonProperty("shocked")]
        public GCondition Shocked { get; init; } = null!;

        [JsonProperty("sleeping")]
        public GCondition Sleeping { get; init; } = null!;

        [JsonProperty("slowness")]
        public GCondition Slowness { get; init; } = null!;

        [JsonProperty("stack")]
        public GCondition Stack { get; init; } = null!;

        [JsonProperty("stoned")]
        public GCondition Stoned { get; init; } = null!;

        [JsonProperty("stunned")]
        public GCondition Stunned { get; init; } = null!;

        [JsonProperty("sugarrush")]
        public GCondition Sugarrush { get; init; } = null!;

        [JsonProperty("tangled")]
        public GCondition Tangled { get; init; } = null!;

        [JsonProperty("town")]
        public GCondition Town { get; init; } = null!;

        [JsonProperty("warcry")]
        public GCondition Warcry { get; init; } = null!;

        [JsonProperty("weakness")]
        public GCondition Weakness { get; init; } = null!;

        [JsonProperty("withdrawal")]
        public GCondition Withdrawal { get; init; } = null!;

        [JsonProperty("woven")]
        public GCondition Woven { get; init; } = null!;

        [JsonProperty("xpower")]
        public GCondition Xpower { get; init; } = null!;

        [JsonProperty("xshotted")]
        public GCondition Xshotted { get; init; } = null!;
    }
}