using Newtonsoft.Json;

namespace AL.Data.Skills
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class SkillsDatum : DatumBase<GSkill>
    {
        [JsonProperty("3shot")]
        public GSkill _3Shot { get; set; } = null!;

        [JsonProperty("4fingers")]
        public GSkill _4Fingers { get; set; } = null!;

        [JsonProperty("5shot")]
        public GSkill _5Shot { get; set; } = null!;

        public GSkill Absorb { get; set; } = null!;
        public GSkill Agitate { get; set; } = null!;
        public GSkill Alchemy { get; set; } = null!;
        public GSkill Anger { get; set; } = null!;
        public GSkill Attack { get; set; } = null!;
        public GSkill Blink { get; set; } = null!;
        public GSkill Burst { get; set; } = null!;
        public GSkill CBurst { get; set; } = null!;
        public GSkill Charge { get; set; } = null!;
        public GSkill Charm { get; set; } = null!;
        public GSkill Cleave { get; set; } = null!;
        public GSkill Curse { get; set; } = null!;

        [JsonProperty("curse_aura")]
        public GSkill CurseAura { get; set; } = null!;

        [JsonProperty("dampening_aura")]
        public GSkill DampeningAura { get; set; } = null!;

        public GSkill DarkBlessing { get; set; } = null!;
        public GSkill DeepFreeze { get; set; } = null!;
        public GSkill Emotion { get; set; } = null!;
        public GSkill Energize { get; set; } = null!;
        public GSkill Entangle { get; set; } = null!;
        public GSkill Esc { get; set; } = null!;
        public GSkill Fishing { get; set; } = null!;
        public GSkill Gm { get; set; } = null!;
        public GSkill HardShell { get; set; } = null!;
        public GSkill Heal { get; set; } = null!;
        public GSkill Healing { get; set; } = null!;
        public GSkill HuntersMark { get; set; } = null!;
        public GSkill Interact { get; set; } = null!;
        public GSkill Invis { get; set; } = null!;
        public GSkill Light { get; set; } = null!;
        public GSkill Magiport { get; set; } = null!;
        public GSkill MassProduction { get; set; } = null!;
        public GSkill MassProductionPP { get; set; } = null!;
        public GSkill MCourage { get; set; } = null!;
        public GSkill MentalBurst { get; set; } = null!;
        public GSkill Mining { get; set; } = null!;
        public GSkill MLight { get; set; } = null!;
        public GSkill MLuck { get; set; } = null!;

        [JsonProperty("move_down")]
        public GSkill MoveDown { get; set; } = null!;

        [JsonProperty("move_left")]
        public GSkill MoveLeft { get; set; } = null!;

        [JsonProperty("move_right")]
        public GSkill MoveRight { get; set; } = null!;

        [JsonProperty("move_up")]
        public GSkill MoveUp { get; set; } = null!;

        public GSkill MShield { get; set; } = null!;
        public GSkill MTangle { get; set; } = null!;

        [JsonProperty("multi_burn")]
        public GSkill MultiBurn { get; set; } = null!;

        [JsonProperty("multi_freeze")]
        public GSkill MultiFreeze { get; set; } = null!;

        [JsonProperty("open_snippet")]
        public GSkill OpenSnippet { get; set; } = null!;

        public GSkill PartyHeal { get; set; } = null!;
        public GSkill PCoat { get; set; } = null!;
        public GSkill PhaseOut { get; set; } = null!;
        public GSkill PiercingShot { get; set; } = null!;
        public GSkill PoisonArrow { get; set; } = null!;
        public GSkill Portal { get; set; } = null!;
        public GSkill Power { get; set; } = null!;

        [JsonProperty("pure_eval")]
        public GSkill PureEval { get; set; } = null!;

        public GSkill QuickPunch { get; set; } = null!;
        public GSkill QuickStab { get; set; } = null!;
        public GSkill Reflection { get; set; } = null!;

        [JsonProperty("regen_hp")]
        public GSkill RegenHP { get; set; } = null!;

        [JsonProperty("regen_mp")]
        public GSkill RegenMp { get; set; } = null!;

        public GSkill Revive { get; set; } = null!;
        public GSkill RSpeed { get; set; } = null!;
        public GSkill Scare { get; set; } = null!;
        public GSkill Selfheal { get; set; } = null!;

        [JsonProperty("self_healing")]
        public GSkill SelfHealing { get; set; } = null!;

        public GSkill Shadowstrike { get; set; } = null!;
        public GSkill Snippet { get; set; } = null!;
        public GSkill Snowball { get; set; } = null!;
        public GSkill Stack { get; set; } = null!;
        public GSkill Stomp { get; set; } = null!;
        public GSkill Stone { get; set; } = null!;
        public GSkill Stop { get; set; } = null!;
        public GSkill SuperShot { get; set; } = null!;
        public GSkill Tangle { get; set; } = null!;
        public GSkill Taunt { get; set; } = null!;

        [JsonProperty("throw")]
        public GSkill Throw { get; set; } = null!;

        [JsonProperty("toggle_character")]
        public GSkill ToggleCharacter { get; set; } = null!;

        [JsonProperty("toggle_code")]
        public GSkill ToggleCode { get; set; } = null!;

        [JsonProperty("toggle_inventory")]
        public GSkill ToggleInventory { get; set; } = null!;

        [JsonProperty("toggle_run_code")]
        public GSkill ToggleRunCode { get; set; } = null!;

        [JsonProperty("toggle_stats")]
        public GSkill ToggleStats { get; set; } = null!;

        public GSkill Track { get; set; } = null!;
        public GSkill Travel { get; set; } = null!;

        [JsonProperty("use_hp")]
        public GSkill UseHP { get; set; } = null!;

        [JsonProperty("use_mp")]
        public GSkill UseMp { get; set; } = null!;

        [JsonProperty("use_town")]
        public GSkill UseTown { get; set; } = null!;

        public GSkill Warcry { get; set; } = null!;
        public GSkill Warp { get; set; } = null!;

        [JsonProperty("weakness_aura")]
        public GSkill WeaknessAura { get; set; } = null!;

        public GSkill XPower { get; set; } = null!;
        public GSkill Zap { get; set; } = null!;
    }
}