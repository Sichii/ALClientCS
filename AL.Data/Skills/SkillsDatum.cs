using Newtonsoft.Json;

namespace AL.Data.Skills
{
    public class SkillsDatum : DatumBase<Skill>
    {
        [JsonProperty("3shot")]
        public Skill _3Shot { get; set; }

        [JsonProperty("4fingers")]
        public Skill _4Fingers { get; set; }

        [JsonProperty("5shot")]
        public Skill _5Shot { get; set; }

        public Skill Absorb { get; set; }
        public Skill Agitate { get; set; }
        public Skill Alchemy { get; set; }
        public Skill Anger { get; set; }
        public Skill Attack { get; set; }
        public Skill Blink { get; set; }
        public Skill Burst { get; set; }
        public Skill CBurst { get; set; }
        public Skill Charge { get; set; }
        public Skill Charm { get; set; }
        public Skill Cleave { get; set; }
        public Skill Curse { get; set; }

        [JsonProperty("curse_aura")]
        public Skill CurseAura { get; set; }

        [JsonProperty("dampening_aura")]
        public Skill DampeningAura { get; set; }

        public Skill DarkBlessing { get; set; }
        public Skill DeepFreeze { get; set; }
        public Skill Emotion { get; set; }
        public Skill Energize { get; set; }
        public Skill Entangle { get; set; }
        public Skill Esc { get; set; }
        public Skill Fishing { get; set; }
        public Skill Gm { get; set; }
        public Skill HardShell { get; set; }
        public Skill Heal { get; set; }
        public Skill Healing { get; set; }
        public Skill HuntersMark { get; set; }
        public Skill Interact { get; set; }
        public Skill Invis { get; set; }
        public Skill Light { get; set; }
        public Skill Magiport { get; set; }
        public Skill MassProduction { get; set; }
        public Skill MassProductionPP { get; set; }
        public Skill MCourage { get; set; }
        public Skill MentalBurst { get; set; }
        public Skill Mining { get; set; }
        public Skill MLight { get; set; }
        public Skill MLuck { get; set; }

        [JsonProperty("move_down")]
        public Skill MoveDown { get; set; }

        [JsonProperty("move_left")]
        public Skill MoveLeft { get; set; }

        [JsonProperty("move_right")]
        public Skill MoveRight { get; set; }

        [JsonProperty("move_up")]
        public Skill MoveUp { get; set; }

        public Skill MShield { get; set; }
        public Skill MTangle { get; set; }

        [JsonProperty("multi_burn")]
        public Skill MultiBurn { get; set; }

        [JsonProperty("multi_freeze")]
        public Skill MultiFreeze { get; set; }

        [JsonProperty("open_snippet")]
        public Skill OpenSnippet { get; set; }

        public Skill PartyHeal { get; set; }
        public Skill PCoat { get; set; }
        public Skill PhaseOut { get; set; }
        public Skill PiercingShot { get; set; }
        public Skill PoisonArrow { get; set; }
        public Skill Portal { get; set; }
        public Skill Power { get; set; }

        [JsonProperty("pure_eval")]
        public Skill PureEval { get; set; }

        public Skill QuickPunch { get; set; }
        public Skill QuickStab { get; set; }
        public Skill Reflection { get; set; }

        [JsonProperty("regen_hp")]
        public Skill RegenHP { get; set; }

        [JsonProperty("regen_mp")]
        public Skill RegenMp { get; set; }

        public Skill Revive { get; set; }
        public Skill RSpeed { get; set; }
        public Skill Scare { get; set; }
        public Skill Selfheal { get; set; }

        [JsonProperty("self_healing")]
        public Skill SelfHealing { get; set; }

        public Skill Shadowstrike { get; set; }
        public Skill Snippet { get; set; }
        public Skill Snowball { get; set; }
        public Skill Stack { get; set; }
        public Skill Stomp { get; set; }
        public Skill Stone { get; set; }
        public Skill Stop { get; set; }
        public Skill SuperShot { get; set; }
        public Skill Tangle { get; set; }
        public Skill Taunt { get; set; }

        [JsonProperty("throw")]
        public Skill Throw { get; set; }

        [JsonProperty("toggle_character")]
        public Skill ToggleCharacter { get; set; }

        [JsonProperty("toggle_code")]
        public Skill ToggleCode { get; set; }

        [JsonProperty("toggle_inventory")]
        public Skill ToggleInventory { get; set; }

        [JsonProperty("toggle_run_code")]
        public Skill ToggleRunCode { get; set; }

        [JsonProperty("toggle_stats")]
        public Skill ToggleStats { get; set; }

        public Skill Track { get; set; }
        public Skill Travel { get; set; }

        [JsonProperty("use_hp")]
        public Skill UseHP { get; set; }

        [JsonProperty("use_mp")]
        public Skill UseMp { get; set; }

        [JsonProperty("use_town")]
        public Skill UseTown { get; set; }

        public Skill Warcry { get; set; }
        public Skill Warp { get; set; }

        [JsonProperty("weakness_aura")]
        public Skill WeaknessAura { get; set; }

        public Skill XPower { get; set; }
        public Skill Zap { get; set; }
    }
}