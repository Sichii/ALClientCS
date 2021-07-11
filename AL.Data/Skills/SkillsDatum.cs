using Newtonsoft.Json;

namespace AL.Data.Skills
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class SkillsDatum : DatumBase<Skill>
    {
        [JsonProperty("3shot")]
        public Skill _3Shot { get; set; } = null!;

        [JsonProperty("4fingers")]
        public Skill _4Fingers { get; set; } = null!;

        [JsonProperty("5shot")]
        public Skill _5Shot { get; set; } = null!;

        public Skill Absorb { get; set; } = null!;
        public Skill Agitate { get; set; } = null!;
        public Skill Alchemy { get; set; } = null!;
        public Skill Anger { get; set; } = null!;
        public Skill Attack { get; set; } = null!;
        public Skill Blink { get; set; } = null!;
        public Skill Burst { get; set; } = null!;
        public Skill CBurst { get; set; } = null!;
        public Skill Charge { get; set; } = null!;
        public Skill Charm { get; set; } = null!;
        public Skill Cleave { get; set; } = null!;
        public Skill Curse { get; set; } = null!;

        [JsonProperty("curse_aura")]
        public Skill CurseAura { get; set; } = null!;

        [JsonProperty("dampening_aura")]
        public Skill DampeningAura { get; set; } = null!;

        public Skill DarkBlessing { get; set; } = null!;
        public Skill DeepFreeze { get; set; } = null!;
        public Skill Emotion { get; set; } = null!;
        public Skill Energize { get; set; } = null!;
        public Skill Entangle { get; set; } = null!;
        public Skill Esc { get; set; } = null!;
        public Skill Fishing { get; set; } = null!;
        public Skill Gm { get; set; } = null!;
        public Skill HardShell { get; set; } = null!;
        public Skill Heal { get; set; } = null!;
        public Skill Healing { get; set; } = null!;
        public Skill HuntersMark { get; set; } = null!;
        public Skill Interact { get; set; } = null!;
        public Skill Invis { get; set; } = null!;
        public Skill Light { get; set; } = null!;
        public Skill Magiport { get; set; } = null!;
        public Skill MassProduction { get; set; } = null!;
        public Skill MassProductionPP { get; set; } = null!;
        public Skill MCourage { get; set; } = null!;
        public Skill MentalBurst { get; set; } = null!;
        public Skill Mining { get; set; } = null!;
        public Skill MLight { get; set; } = null!;
        public Skill MLuck { get; set; } = null!;

        [JsonProperty("move_down")]
        public Skill MoveDown { get; set; } = null!;

        [JsonProperty("move_left")]
        public Skill MoveLeft { get; set; } = null!;

        [JsonProperty("move_right")]
        public Skill MoveRight { get; set; } = null!;

        [JsonProperty("move_up")]
        public Skill MoveUp { get; set; } = null!;

        public Skill MShield { get; set; } = null!;
        public Skill MTangle { get; set; } = null!;

        [JsonProperty("multi_burn")]
        public Skill MultiBurn { get; set; } = null!;

        [JsonProperty("multi_freeze")]
        public Skill MultiFreeze { get; set; } = null!;

        [JsonProperty("open_snippet")]
        public Skill OpenSnippet { get; set; } = null!;

        public Skill PartyHeal { get; set; } = null!;
        public Skill PCoat { get; set; } = null!;
        public Skill PhaseOut { get; set; } = null!;
        public Skill PiercingShot { get; set; } = null!;
        public Skill PoisonArrow { get; set; } = null!;
        public Skill Portal { get; set; } = null!;
        public Skill Power { get; set; } = null!;

        [JsonProperty("pure_eval")]
        public Skill PureEval { get; set; } = null!;

        public Skill QuickPunch { get; set; } = null!;
        public Skill QuickStab { get; set; } = null!;
        public Skill Reflection { get; set; } = null!;

        [JsonProperty("regen_hp")]
        public Skill RegenHP { get; set; } = null!;

        [JsonProperty("regen_mp")]
        public Skill RegenMp { get; set; } = null!;

        public Skill Revive { get; set; } = null!;
        public Skill RSpeed { get; set; } = null!;
        public Skill Scare { get; set; } = null!;
        public Skill Selfheal { get; set; } = null!;

        [JsonProperty("self_healing")]
        public Skill SelfHealing { get; set; } = null!;

        public Skill Shadowstrike { get; set; } = null!;
        public Skill Snippet { get; set; } = null!;
        public Skill Snowball { get; set; } = null!;
        public Skill Stack { get; set; } = null!;
        public Skill Stomp { get; set; } = null!;
        public Skill Stone { get; set; } = null!;
        public Skill Stop { get; set; } = null!;
        public Skill SuperShot { get; set; } = null!;
        public Skill Tangle { get; set; } = null!;
        public Skill Taunt { get; set; } = null!;

        [JsonProperty("throw")]
        public Skill Throw { get; set; } = null!;

        [JsonProperty("toggle_character")]
        public Skill ToggleCharacter { get; set; } = null!;

        [JsonProperty("toggle_code")]
        public Skill ToggleCode { get; set; } = null!;

        [JsonProperty("toggle_inventory")]
        public Skill ToggleInventory { get; set; } = null!;

        [JsonProperty("toggle_run_code")]
        public Skill ToggleRunCode { get; set; } = null!;

        [JsonProperty("toggle_stats")]
        public Skill ToggleStats { get; set; } = null!;

        public Skill Track { get; set; } = null!;
        public Skill Travel { get; set; } = null!;

        [JsonProperty("use_hp")]
        public Skill UseHP { get; set; } = null!;

        [JsonProperty("use_mp")]
        public Skill UseMp { get; set; } = null!;

        [JsonProperty("use_town")]
        public Skill UseTown { get; set; } = null!;

        public Skill Warcry { get; set; } = null!;
        public Skill Warp { get; set; } = null!;

        [JsonProperty("weakness_aura")]
        public Skill WeaknessAura { get; set; } = null!;

        public Skill XPower { get; set; } = null!;
        public Skill Zap { get; set; } = null!;
    }
}