using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Conditions
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GCondition>))]
    public class ConditionsDatum : DatumBase<GCondition>
    {
        public GCondition Authfail { get; init; } = null!;
        public GCondition Blink { get; init; } = null!;
        public GCondition Burned { get; init; } = null!;
        public GCondition Charging { get; init; } = null!;
        public GCondition Charmed { get; init; } = null!;
        public GCondition Cursed { get; init; } = null!;
        public GCondition Dampened { get; init; } = null!;
        public GCondition Darkblessing { get; init; } = null!;
        public GCondition Deepfreezed { get; init; } = null!;
        public GCondition Easterluck { get; init; } = null!;
        public GCondition Eburn { get; init; } = null!;
        public GCondition Eheal { get; init; } = null!;
        public GCondition Energized { get; init; } = null!;
        public GCondition Fingered { get; init; } = null!;
        public GCondition Fishing { get; init; } = null!;
        public GCondition Frozen { get; init; } = null!;
        public GCondition Fullguard { get; init; } = null!;
        public GCondition Hardshell { get; init; } = null!;
        public GCondition Holidayspirit { get; init; } = null!;
        public GCondition Invincible { get; init; } = null!;
        public GCondition Invis { get; init; } = null!;
        [JsonProperty("licenced")]
        public GCondition Licensed { get; init; } = null!;
        public GCondition Marked { get; init; } = null!;
        public GCondition Massproduction { get; init; } = null!;
        public GCondition Massproductionpp { get; init; } = null!;
        public GCondition Mcourage { get; init; } = null!;
        public GCondition Mlifesteal { get; init; } = null!;
        public GCondition Mluck { get; init; } = null!;
        public GCondition Monsterhunt { get; init; } = null!;
        public GCondition Mshield { get; init; } = null!;
        public GCondition Newcomersblessing { get; init; } = null!;
        public GCondition Notverified { get; init; } = null!;
        public GCondition Phasedout { get; init; } = null!;
        public GCondition Poisoned { get; init; } = null!;
        public GCondition Poisonous { get; init; } = null!;
        public GCondition Power { get; init; } = null!;
        public GCondition Reflection { get; init; } = null!;
        public GCondition Rspeed { get; init; } = null!;
        public GCondition Sanguine { get; init; } = null!;
        public GCondition Shocked { get; init; } = null!;
        public GCondition Slowness { get; init; } = null!;
        public GCondition Stack { get; init; } = null!;
        public GCondition Stoned { get; init; } = null!;
        public GCondition Stunned { get; init; } = null!;
        public GCondition Sugarrush { get; init; } = null!;
        public GCondition Tangled { get; init; } = null!;
        public GCondition Town { get; init; } = null!;
        public GCondition Warcry { get; init; } = null!;
        public GCondition Weakness { get; init; } = null!;
        public GCondition Withdrawal { get; init; } = null!;
        public GCondition Xpower { get; init; } = null!;
        public GCondition Xshotted { get; init; } = null!;
    }
}