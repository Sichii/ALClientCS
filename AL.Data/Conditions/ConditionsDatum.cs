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
        public GCondition AuthFail { get; set; } = null!;
        public GCondition Blink { get; set; } = null!;
        public GCondition Burned { get; set; } = null!;
        public GCondition Charging { get; set; } = null!;
        public GCondition Charmed { get; set; } = null!;
        public GCondition Cursed { get; set; } = null!;
        public GCondition Dampened { get; set; } = null!;
        public GCondition DarkBlessing { get; set; } = null!;
        public GCondition DeepFreezed { get; set; } = null!;
        public GCondition EasterLuck { get; set; } = null!;
        public GCondition EBurn { get; set; } = null!;
        public GCondition EHeal { get; set; } = null!;
        public GCondition Energized { get; set; } = null!;
        public GCondition Fingered { get; set; } = null!;
        public GCondition Fishing { get; set; } = null!;
        public GCondition Frozen { get; set; } = null!;
        public GCondition FullGuard { get; set; } = null!;
        public GCondition HardShell { get; set; } = null!;
        public GCondition HolidaySpirit { get; set; } = null!;
        public GCondition Invincible { get; set; } = null!;
        public GCondition Invis { get; set; } = null!;

        [JsonProperty("licenced")]
        public GCondition Licensed { get; set; } = null!;

        public GCondition Marked { get; set; } = null!;
        public GCondition MassProduction { get; set; } = null!;
        public GCondition MassProductionPP { get; set; } = null!;
        public GCondition MCourage { get; set; } = null!;
        public GCondition MLifeSteal { get; set; } = null!;
        public GCondition MLuck { get; set; } = null!;
        public GCondition MonsterHunt { get; set; } = null!;
        public GCondition MShield { get; set; } = null!;
        public GCondition NewcomersBlessing { get; set; } = null!;
        public GCondition NotVerified { get; set; } = null!;
        public GCondition PhasedOut { get; set; } = null!;
        public GCondition Poisoned { get; set; } = null!;
        public GCondition Poisonous { get; set; } = null!;
        public GCondition Power { get; set; } = null!;
        public GCondition Reflection { get; set; } = null!;
        public GCondition RSpeed { get; set; } = null!;
        public GCondition Sanguine { get; set; } = null!;
        public GCondition Shocked { get; set; } = null!;
        public GCondition Slowness { get; set; } = null!;
        public GCondition Stack { get; set; } = null!;
        public GCondition Stoned { get; set; } = null!;
        public GCondition Stunned { get; set; } = null!;
        public GCondition SugarRush { get; set; } = null!;
        public GCondition Tangled { get; set; } = null!;
        public GCondition Town { get; set; } = null!;
        public GCondition WarCry { get; set; } = null!;
        public GCondition Weakness { get; set; } = null!;
        public GCondition Withdrawal { get; set; } = null!;
        public GCondition XPower { get; set; } = null!;
        public GCondition XShotted { get; set; } = null!;
    }
}