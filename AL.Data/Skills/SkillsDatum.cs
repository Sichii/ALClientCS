#region
using Newtonsoft.Json;
#endregion

namespace AL.Data.Skills
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class SkillsDatum : DatumBase<GSkill>
    {
        [JsonProperty("3shot")]
        public GSkill _3Shot { get; init; } = null!;

        [JsonProperty("4fingers")]
        public GSkill _4Fingers { get; init; } = null!;

        [JsonProperty("5shot")]
        public GSkill _5Shot { get; init; } = null!;

        [JsonProperty("absorb")]
        public GSkill Absorb { get; init; } = null!;

        [JsonProperty("agitate")]
        public GSkill Agitate { get; init; } = null!;

        [JsonProperty("alchemy")]
        public GSkill Alchemy { get; init; } = null!;

        [JsonProperty("anger")]
        public GSkill Anger { get; init; } = null!;

        [JsonProperty("attack")]
        public GSkill Attack { get; init; } = null!;

        [JsonProperty("blink")]
        public GSkill Blink { get; init; } = null!;

        [JsonProperty("burst")]
        public GSkill Burst { get; init; } = null!;

        [JsonProperty("cburst")]
        public GSkill Cburst { get; init; } = null!;

        [JsonProperty("charge")]
        public GSkill Charge { get; init; } = null!;

        [JsonProperty("charm")]
        public GSkill Charm { get; init; } = null!;

        [JsonProperty("cleave")]
        public GSkill Cleave { get; init; } = null!;

        [JsonProperty("curse")]
        public GSkill Curse { get; init; } = null!;

        [JsonProperty("curse_aura")]
        public GSkill CurseAura { get; init; } = null!;

        [JsonProperty("dampening_aura")]
        public GSkill DampeningAura { get; init; } = null!;

        [JsonProperty("darkblessing")]
        public GSkill Darkblessing { get; init; } = null!;

        [JsonProperty("dash")]
        public GSkill Dash { get; init; } = null!;

        [JsonProperty("deepfreeze")]
        public GSkill Deepfreeze { get; init; } = null!;

        [JsonProperty("emotion")]
        public GSkill Emotion { get; init; } = null!;

        [JsonProperty("energize")]
        public GSkill Energize { get; init; } = null!;

        [JsonProperty("entangle")]
        public GSkill Entangle { get; init; } = null!;

        [JsonProperty("esc")]
        public GSkill Esc { get; init; } = null!;

        [JsonProperty("fireball")]
        public GSkill Fireball { get; init; } = null!;

        [JsonProperty("fishing")]
        public GSkill Fishing { get; init; } = null!;

        [JsonProperty("frostball")]
        public GSkill Frostball { get; init; } = null!;

        [JsonProperty("gm")]
        public GSkill Gm { get; init; } = null!;

        [JsonProperty("hardshell")]
        public GSkill Hardshell { get; init; } = null!;

        [JsonProperty("heal")]
        public GSkill Heal { get; init; } = null!;

        [JsonProperty("healing")]
        public GSkill Healing { get; init; } = null!;

        [JsonProperty("huntersmark")]
        public GSkill Huntersmark { get; init; } = null!;

        [JsonProperty("interact")]
        public GSkill Interact { get; init; } = null!;

        [JsonProperty("invis")]
        public GSkill Invis { get; init; } = null!;

        [JsonProperty("light")]
        public GSkill Light { get; init; } = null!;

        [JsonProperty("magiport")]
        public GSkill Magiport { get; init; } = null!;

        [JsonProperty("massproduction")]
        public GSkill Massproduction { get; init; } = null!;

        [JsonProperty("massproductionpp")]
        public GSkill Massproductionpp { get; init; } = null!;

        [JsonProperty("mcourage")]
        public GSkill Mcourage { get; init; } = null!;

        [JsonProperty("mentalburst")]
        public GSkill Mentalburst { get; init; } = null!;

        [JsonProperty("mfrenzy")]
        public GSkill Mfrenzy { get; init; } = null!;

        [JsonProperty("mining")]
        public GSkill Mining { get; init; } = null!;

        [JsonProperty("mlight")]
        public GSkill Mlight { get; init; } = null!;

        [JsonProperty("mluck")]
        public GSkill Mluck { get; init; } = null!;

        [JsonProperty("move_down")]
        public GSkill MoveDown { get; init; } = null!;

        [JsonProperty("move_left")]
        public GSkill MoveLeft { get; init; } = null!;

        [JsonProperty("move_right")]
        public GSkill MoveRight { get; init; } = null!;

        [JsonProperty("move_up")]
        public GSkill MoveUp { get; init; } = null!;

        [JsonProperty("mshield")]
        public GSkill Mshield { get; init; } = null!;

        [JsonProperty("mtangle")]
        public GSkill Mtangle { get; init; } = null!;

        [JsonProperty("multi_burn")]
        public GSkill MultiBurn { get; init; } = null!;

        [JsonProperty("multi_freeze")]
        public GSkill MultiFreeze { get; init; } = null!;

        [JsonProperty("open_snippet")]
        public GSkill OpenSnippet { get; init; } = null!;

        [JsonProperty("partyheal")]
        public GSkill Partyheal { get; init; } = null!;

        [JsonProperty("pcoat")]
        public GSkill Pcoat { get; init; } = null!;

        [JsonProperty("phaseout")]
        public GSkill Phaseout { get; init; } = null!;

        [JsonProperty("pickpocket")]
        public GSkill Pickpocket { get; init; } = null!;

        [JsonProperty("piercingshot")]
        public GSkill Piercingshot { get; init; } = null!;

        [JsonProperty("poisonarrow")]
        public GSkill Poisonarrow { get; init; } = null!;

        [JsonProperty("portal")]
        public GSkill Portal { get; init; } = null!;

        [JsonProperty("power")]
        public GSkill Power { get; init; } = null!;

        [JsonProperty("pure_eval")]
        public GSkill PureEval { get; init; } = null!;

        [JsonProperty("purify")]
        public GSkill Purify { get; init; } = null!;

        [JsonProperty("quickpunch")]
        public GSkill Quickpunch { get; init; } = null!;

        [JsonProperty("quickstab")]
        public GSkill Quickstab { get; init; } = null!;

        [JsonProperty("reflection")]
        public GSkill Reflection { get; init; } = null!;

        [JsonProperty("regen_hp")]
        public GSkill RegenHp { get; init; } = null!;

        [JsonProperty("regen_mp")]
        public GSkill RegenMp { get; init; } = null!;

        [JsonProperty("revive")]
        public GSkill Revive { get; init; } = null!;

        [JsonProperty("rspeed")]
        public GSkill Rspeed { get; init; } = null!;

        [JsonProperty("scare")]
        public GSkill Scare { get; init; } = null!;

        [JsonProperty("selfheal")]
        public GSkill Selfheal { get; init; } = null!;

        [JsonProperty("self_healing")]
        public GSkill SelfHealing { get; init; } = null!;

        [JsonProperty("shadowstrike")]
        public GSkill Shadowstrike { get; init; } = null!;

        [JsonProperty("smash")]
        public GSkill Smash { get; init; } = null!;

        [JsonProperty("snippet")]
        public GSkill Snippet { get; init; } = null!;

        [JsonProperty("snowball")]
        public GSkill Snowball { get; init; } = null!;

        [JsonProperty("stack")]
        public GSkill Stack { get; init; } = null!;

        [JsonProperty("stomp")]
        public GSkill Stomp { get; init; } = null!;

        [JsonProperty("stone")]
        public GSkill Stone { get; init; } = null!;

        [JsonProperty("stop")]
        public GSkill Stop { get; init; } = null!;

        [JsonProperty("supershot")]
        public GSkill Supershot { get; init; } = null!;

        [JsonProperty("tangle")]
        public GSkill Tangle { get; init; } = null!;

        [JsonProperty("taunt")]
        public GSkill Taunt { get; init; } = null!;

        [JsonProperty("throw")]
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

        [JsonProperty("track")]
        public GSkill Track { get; init; } = null!;

        [JsonProperty("travel")]
        public GSkill Travel { get; init; } = null!;

        [JsonProperty("use_hp")]
        public GSkill UseHp { get; init; } = null!;

        [JsonProperty("use_mp")]
        public GSkill UseMp { get; init; } = null!;

        [JsonProperty("use_town")]
        public GSkill UseTown { get; init; } = null!;

        [JsonProperty("warcry")]
        public GSkill Warcry { get; init; } = null!;

        [JsonProperty("warp")]
        public GSkill Warp { get; init; } = null!;

        [JsonProperty("warpstomp")]
        public GSkill Warpstomp { get; init; } = null!;

        [JsonProperty("weakness_aura")]
        public GSkill WeaknessAura { get; init; } = null!;

        [JsonProperty("xpower")]
        public GSkill Xpower { get; init; } = null!;

        [JsonProperty("zap")]
        public GSkill Zap { get; init; } = null!;

        [JsonProperty("zapperzap")]
        public GSkill Zapperzap { get; init; } = null!;
    }
}