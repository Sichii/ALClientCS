using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Conditions
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<Condition>))]
    public class ConditionsDatum : DatumBase<Condition>
    {
        public Condition AuthFail { get; set; } = null!;
        public Condition Blink { get; set; } = null!;
        public Condition Burned { get; set; } = null!;
        public Condition Charging { get; set; } = null!;
        public Condition Charmed { get; set; } = null!;
        public Condition Cursed { get; set; } = null!;
        public Condition Dampened { get; set; } = null!;
        public Condition DarkBlessing { get; set; } = null!;
        public Condition DeepFreezed { get; set; } = null!;
        public Condition EasterLuck { get; set; } = null!;
        public Condition EBurn { get; set; } = null!;
        public Condition EHeal { get; set; } = null!;
        public Condition Energized { get; set; } = null!;
        public Condition Fingered { get; set; } = null!;
        public Condition Fishing { get; set; } = null!;
        public Condition Frozen { get; set; } = null!;
        public Condition FullGuard { get; set; } = null!;
        public Condition HardShell { get; set; } = null!;
        public Condition HolidaySpirit { get; set; } = null!;
        public Condition Invincible { get; set; } = null!;
        public Condition Invis { get; set; } = null!;

        [JsonProperty("licenced")]
        public Condition Licensed { get; set; } = null!;

        public Condition Marked { get; set; } = null!;
        public Condition MassProduction { get; set; } = null!;
        public Condition MassProductionPP { get; set; } = null!;
        public Condition MCourage { get; set; } = null!;
        public Condition MLifeSteal { get; set; } = null!;
        public Condition MLuck { get; set; } = null!;
        public Condition MonsterHunt { get; set; } = null!;
        public Condition MShield { get; set; } = null!;
        public Condition NewcomersBlessing { get; set; } = null!;
        public Condition NotVerified { get; set; } = null!;
        public Condition PhasedOut { get; set; } = null!;
        public Condition Poisoned { get; set; } = null!;
        public Condition Poisonous { get; set; } = null!;
        public Condition Power { get; set; } = null!;
        public Condition Reflection { get; set; } = null!;
        public Condition RSpeed { get; set; } = null!;
        public Condition Sanguine { get; set; } = null!;
        public Condition Shocked { get; set; } = null!;
        public Condition Slowness { get; set; } = null!;
        public Condition Stack { get; set; } = null!;
        public Condition Stoned { get; set; } = null!;
        public Condition Stunned { get; set; } = null!;
        public Condition SugarRush { get; set; } = null!;
        public Condition Tangled { get; set; } = null!;
        public Condition Town { get; set; } = null!;
        public Condition WarCry { get; set; } = null!;
        public Condition Weakness { get; set; } = null!;
        public Condition Withdrawal { get; set; } = null!;
        public Condition XPower { get; set; } = null!;
        public Condition XShotted { get; set; } = null!;
    }
}