using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Conditions
{
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<Condition>))]
    public class ConditionsDatum : DatumBase<Condition>
    {
        public Condition AuthFail { get; set; }
        public Condition Blink { get; set; }
        public Condition Burned { get; set; }
        public Condition Charging { get; set; }
        public Condition Charmed { get; set; }
        public Condition Cursed { get; set; }
        public Condition Dampened { get; set; }
        public Condition DarkBlessing { get; set; }
        public Condition DeepFreezed { get; set; }
        public Condition EasterLuck { get; set; }
        public Condition EBurn { get; set; }
        public Condition EHeal { get; set; }
        public Condition Energized { get; set; }
        public Condition Fingered { get; set; }
        public Condition Fishing { get; set; }
        public Condition Frozen { get; set; }
        public Condition FullGuard { get; set; }
        public Condition HardShell { get; set; }
        public Condition HolidaySpirit { get; set; }
        public Condition Invincible { get; set; }
        public Condition Invis { get; set; }

        [JsonProperty("licenced")]
        public Condition Licensed { get; set; }

        public Condition Marked { get; set; }
        public Condition MassProduction { get; set; }
        public Condition MassProductionPP { get; set; }
        public Condition MCourage { get; set; }
        public Condition MLifeSteal { get; set; }
        public Condition MLuck { get; set; }
        public Condition MonsterHunt { get; set; }
        public Condition MShield { get; set; }
        public Condition NewcomersBlessing { get; set; }
        public Condition NotVerified { get; set; }
        public Condition PhasedOut { get; set; }
        public Condition Poisoned { get; set; }
        public Condition Poisonous { get; set; }
        public Condition Power { get; set; }
        public Condition Reflection { get; set; }
        public Condition RSpeed { get; set; }
        public Condition Sanguine { get; set; }
        public Condition Shocked { get; set; }
        public Condition Slowness { get; set; }
        public Condition Stack { get; set; }
        public Condition Stoned { get; set; }
        public Condition Stunned { get; set; }
        public Condition SugarRush { get; set; }
        public Condition Tangled { get; set; }
        public Condition Town { get; set; }
        public Condition WarCry { get; set; }
        public Condition Weakness { get; set; }
        public Condition Withdrawal { get; set; }
        public Condition XPower { get; set; }
        public Condition XShotted { get; set; }
    }
}