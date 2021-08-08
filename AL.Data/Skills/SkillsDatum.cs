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
        public GSkill _3shot { get; init; } = null!;
        [JsonProperty("4fingers")]
        public GSkill _4fingers { get; init; } = null!;
        [JsonProperty("5shot")]
        public GSkill _5shot { get; init; } = null!;
        public GSkill Absorb { get; init; } = null!;
        public GSkill Agitate { get; init; } = null!;
        public GSkill Alchemy { get; init; } = null!;
        public GSkill Anger { get; init; } = null!;
        public GSkill Attack { get; init; } = null!;
        public GSkill Blink { get; init; } = null!;
        public GSkill Burst { get; init; } = null!;
        public GSkill Cburst { get; init; } = null!;
        public GSkill Charge { get; init; } = null!;
        public GSkill Charm { get; init; } = null!;
        public GSkill Cleave { get; init; } = null!;
        public GSkill Curse { get; init; } = null!;
        [JsonProperty("curse_aura")]
        public GSkill CurseAura { get; init; } = null!;
        [JsonProperty("dampening_aura")]
        public GSkill DampeningAura { get; init; } = null!;
        public GSkill Darkblessing { get; init; } = null!;
        public GSkill Deepfreeze { get; init; } = null!;
        public GSkill Emotion { get; init; } = null!;
        public GSkill Energize { get; init; } = null!;
        public GSkill Entangle { get; init; } = null!;
        public GSkill Esc { get; init; } = null!;
        public GSkill Fishing { get; init; } = null!;
        public GSkill Gm { get; init; } = null!;
        public GSkill Hardshell { get; init; } = null!;
        public GSkill Heal { get; init; } = null!;
        public GSkill Healing { get; init; } = null!;
        public GSkill Huntersmark { get; init; } = null!;
        public GSkill Interact { get; init; } = null!;
        public GSkill Invis { get; init; } = null!;
        public GSkill Light { get; init; } = null!;
        public GSkill Magiport { get; init; } = null!;
        public GSkill Massproduction { get; init; } = null!;
        public GSkill Massproductionpp { get; init; } = null!;
        public GSkill Mcourage { get; init; } = null!;
        public GSkill Mentalburst { get; init; } = null!;
        public GSkill Mining { get; init; } = null!;
        public GSkill Mlight { get; init; } = null!;
        public GSkill Mluck { get; init; } = null!;
        [JsonProperty("move_down")]
        public GSkill MoveDown { get; init; } = null!;
        [JsonProperty("move_left")]
        public GSkill MoveLeft { get; init; } = null!;
        [JsonProperty("move_right")]
        public GSkill MoveRight { get; init; } = null!;
        [JsonProperty("move_up")]
        public GSkill MoveUp { get; init; } = null!;
        public GSkill Mshield { get; init; } = null!;
        public GSkill Mtangle { get; init; } = null!;
        [JsonProperty("multi_burn")]
        public GSkill MultiBurn { get; init; } = null!;
        [JsonProperty("multi_freeze")]
        public GSkill MultiFreeze { get; init; } = null!;
        [JsonProperty("open_snippet")]
        public GSkill OpenSnippet { get; init; } = null!;
        public GSkill Partyheal { get; init; } = null!;
        public GSkill Pcoat { get; init; } = null!;
        public GSkill Phaseout { get; init; } = null!;
        public GSkill Piercingshot { get; init; } = null!;
        public GSkill Poisonarrow { get; init; } = null!;
        public GSkill Portal { get; init; } = null!;
        public GSkill Power { get; init; } = null!;
        [JsonProperty("pure_eval")]
        public GSkill PureEval { get; init; } = null!;
        public GSkill Quickpunch { get; init; } = null!;
        public GSkill Quickstab { get; init; } = null!;
        public GSkill Reflection { get; init; } = null!;
        [JsonProperty("regen_hp")]
        public GSkill RegenHp { get; init; } = null!;
        [JsonProperty("regen_mp")]
        public GSkill RegenMp { get; init; } = null!;
        public GSkill Revive { get; init; } = null!;
        public GSkill Rspeed { get; init; } = null!;
        public GSkill Scare { get; init; } = null!;
        public GSkill Selfheal { get; init; } = null!;
        [JsonProperty("self_healing")]
        public GSkill SelfHealing { get; init; } = null!;
        public GSkill Shadowstrike { get; init; } = null!;
        public GSkill Snippet { get; init; } = null!;
        public GSkill Snowball { get; init; } = null!;
        public GSkill Stack { get; init; } = null!;
        public GSkill Stomp { get; init; } = null!;
        public GSkill Stone { get; init; } = null!;
        public GSkill Stop { get; init; } = null!;
        public GSkill Supershot { get; init; } = null!;
        public GSkill Tangle { get; init; } = null!;
        public GSkill Taunt { get; init; } = null!;
        public GSkill Throw { get; init; } = null!;
        [JsonProperty("toggle_character")]
        public GSkill ToggleCharacter { get; init; } = null!;
        [JsonProperty("toggle_code")]
        public GSkill ToggleCode { get; init; } = null!;
        [JsonProperty("toggle_inventory")]
        public GSkill ToggleInventory { get; init; } = null!;
        [JsonProperty("toggle_run_code")]
        public GSkill ToggleRunCode { get; init; } = null!;
        [JsonProperty("toggle_stats")]
        public GSkill ToggleStats { get; init; } = null!;
        public GSkill Track { get; init; } = null!;
        public GSkill Travel { get; init; } = null!;
        [JsonProperty("use_hp")]
        public GSkill UseHp { get; init; } = null!;
        [JsonProperty("use_mp")]
        public GSkill UseMp { get; init; } = null!;
        [JsonProperty("use_town")]
        public GSkill UseTown { get; init; } = null!;
        public GSkill Warcry { get; init; } = null!;
        public GSkill Warp { get; init; } = null!;
        public GSkill Warpstomp { get; init; } = null!;
        [JsonProperty("weakness_aura")]
        public GSkill WeaknessAura { get; init; } = null!;
        public GSkill Xpower { get; init; } = null!;
        public GSkill Zap { get; init; } = null!;
        public GSkill Zapperzap { get; init; } = null!;
    }
}