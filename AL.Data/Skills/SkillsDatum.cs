#region
using System.Text.Json.Serialization;
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
        [JsonPropertyName("3shot")]
        public GSkill _3Shot { get; init; } = null!;

        [JsonProperty("4fingers")]
        [JsonPropertyName("4fingers")]
        public GSkill _4Fingers { get; init; } = null!;

        [JsonProperty("5shot")]
        [JsonPropertyName("5shot")]
        public GSkill _5Shot { get; init; } = null!;

        [JsonProperty("absorb")]
        [JsonPropertyName("absorb")]
        public GSkill Absorb { get; init; } = null!;

        [JsonProperty("agitate")]
        [JsonPropertyName("agitate")]
        public GSkill Agitate { get; init; } = null!;

        [JsonProperty("alchemy")]
        [JsonPropertyName("alchemy")]
        public GSkill Alchemy { get; init; } = null!;

        [JsonProperty("anger")]
        [JsonPropertyName("anger")]
        public GSkill Anger { get; init; } = null!;

        [JsonProperty("attack")]
        [JsonPropertyName("attack")]
        public GSkill Attack { get; init; } = null!;

        [JsonProperty("blink")]
        [JsonPropertyName("blink")]
        public GSkill Blink { get; init; } = null!;

        [JsonProperty("burst")]
        [JsonPropertyName("burst")]
        public GSkill Burst { get; init; } = null!;

        [JsonProperty("cburst")]
        [JsonPropertyName("cburst")]
        public GSkill Cburst { get; init; } = null!;

        [JsonProperty("charge")]
        [JsonPropertyName("charge")]
        public GSkill Charge { get; init; } = null!;

        [JsonProperty("charm")]
        [JsonPropertyName("charm")]
        public GSkill Charm { get; init; } = null!;

        [JsonProperty("cleave")]
        [JsonPropertyName("cleave")]
        public GSkill Cleave { get; init; } = null!;

        [JsonProperty("curse")]
        [JsonPropertyName("curse")]
        public GSkill Curse { get; init; } = null!;

        [JsonProperty("curse_aura")]
        [JsonPropertyName("curse_aura")]
        public GSkill CurseAura { get; init; } = null!;

        [JsonProperty("dampening_aura")]
        [JsonPropertyName("dampening_aura")]
        public GSkill DampeningAura { get; init; } = null!;

        [JsonProperty("darkblessing")]
        [JsonPropertyName("darkblessing")]
        public GSkill Darkblessing { get; init; } = null!;

        [JsonProperty("dash")]
        [JsonPropertyName("dash")]
        public GSkill Dash { get; init; } = null!;

        [JsonProperty("deepfreeze")]
        [JsonPropertyName("deepfreeze")]
        public GSkill Deepfreeze { get; init; } = null!;

        [JsonProperty("emotion")]
        [JsonPropertyName("emotion")]
        public GSkill Emotion { get; init; } = null!;

        [JsonProperty("energize")]
        [JsonPropertyName("energize")]
        public GSkill Energize { get; init; } = null!;

        [JsonProperty("entangle")]
        [JsonPropertyName("entangle")]
        public GSkill Entangle { get; init; } = null!;

        [JsonProperty("esc")]
        [JsonPropertyName("esc")]
        public GSkill Esc { get; init; } = null!;

        [JsonProperty("fireball")]
        [JsonPropertyName("fireball")]
        public GSkill Fireball { get; init; } = null!;

        [JsonProperty("fishing")]
        [JsonPropertyName("fishing")]
        public GSkill Fishing { get; init; } = null!;

        [JsonProperty("frostball")]
        [JsonPropertyName("frostball")]
        public GSkill Frostball { get; init; } = null!;

        [JsonProperty("gm")]
        [JsonPropertyName("gm")]
        public GSkill Gm { get; init; } = null!;

        [JsonProperty("hardshell")]
        [JsonPropertyName("hardshell")]
        public GSkill Hardshell { get; init; } = null!;

        [JsonProperty("heal")]
        [JsonPropertyName("heal")]
        public GSkill Heal { get; init; } = null!;

        [JsonProperty("healing")]
        [JsonPropertyName("healing")]
        public GSkill Healing { get; init; } = null!;

        [JsonProperty("huntersmark")]
        [JsonPropertyName("huntersmark")]
        public GSkill Huntersmark { get; init; } = null!;

        [JsonProperty("interact")]
        [JsonPropertyName("interact")]
        public GSkill Interact { get; init; } = null!;

        [JsonProperty("invis")]
        [JsonPropertyName("invis")]
        public GSkill Invis { get; init; } = null!;

        [JsonProperty("light")]
        [JsonPropertyName("light")]
        public GSkill Light { get; init; } = null!;

        [JsonProperty("magiport")]
        [JsonPropertyName("magiport")]
        public GSkill Magiport { get; init; } = null!;

        [JsonProperty("massexchange")]
        [JsonPropertyName("massexchange")]
        public GSkill Massexchange { get; init; } = null!;

        [JsonProperty("massexchangepp")]
        [JsonPropertyName("massexchangepp")]
        public GSkill Massexchangepp { get; init; } = null!;

        [JsonProperty("massproduction")]
        [JsonPropertyName("massproduction")]
        public GSkill Massproduction { get; init; } = null!;

        [JsonProperty("massproductionpp")]
        [JsonPropertyName("massproductionpp")]
        public GSkill Massproductionpp { get; init; } = null!;

        [JsonProperty("mcourage")]
        [JsonPropertyName("mcourage")]
        public GSkill Mcourage { get; init; } = null!;

        [JsonProperty("mentalburst")]
        [JsonPropertyName("mentalburst")]
        public GSkill Mentalburst { get; init; } = null!;

        [JsonProperty("mfrenzy")]
        [JsonPropertyName("mfrenzy")]
        public GSkill Mfrenzy { get; init; } = null!;

        [JsonProperty("mining")]
        [JsonPropertyName("mining")]
        public GSkill Mining { get; init; } = null!;

        [JsonProperty("mlight")]
        [JsonPropertyName("mlight")]
        public GSkill Mlight { get; init; } = null!;

        [JsonProperty("mluck")]
        [JsonPropertyName("mluck")]
        public GSkill Mluck { get; init; } = null!;

        [JsonProperty("move_down")]
        [JsonPropertyName("move_down")]
        public GSkill MoveDown { get; init; } = null!;

        [JsonProperty("move_left")]
        [JsonPropertyName("move_left")]
        public GSkill MoveLeft { get; init; } = null!;

        [JsonProperty("move_right")]
        [JsonPropertyName("move_right")]
        public GSkill MoveRight { get; init; } = null!;

        [JsonProperty("move_up")]
        [JsonPropertyName("move_up")]
        public GSkill MoveUp { get; init; } = null!;

        [JsonProperty("mshield")]
        [JsonPropertyName("mshield")]
        public GSkill Mshield { get; init; } = null!;

        [JsonProperty("mtangle")]
        [JsonPropertyName("mtangle")]
        public GSkill Mtangle { get; init; } = null!;

        [JsonProperty("multi_burn")]
        [JsonPropertyName("multi_burn")]
        public GSkill MultiBurn { get; init; } = null!;

        [JsonProperty("multi_freeze")]
        [JsonPropertyName("multi_freeze")]
        public GSkill MultiFreeze { get; init; } = null!;

        [JsonProperty("open_snippet")]
        [JsonPropertyName("open_snippet")]
        public GSkill OpenSnippet { get; init; } = null!;

        [JsonProperty("partyheal")]
        [JsonPropertyName("partyheal")]
        public GSkill Partyheal { get; init; } = null!;

        [JsonProperty("pcoat")]
        [JsonPropertyName("pcoat")]
        public GSkill Pcoat { get; init; } = null!;

        [JsonProperty("phaseout")]
        [JsonPropertyName("phaseout")]
        public GSkill Phaseout { get; init; } = null!;

        [JsonProperty("pickpocket")]
        [JsonPropertyName("pickpocket")]
        public GSkill Pickpocket { get; init; } = null!;

        [JsonProperty("piercingshot")]
        [JsonPropertyName("piercingshot")]
        public GSkill Piercingshot { get; init; } = null!;

        [JsonProperty("poisonarrow")]
        [JsonPropertyName("poisonarrow")]
        public GSkill Poisonarrow { get; init; } = null!;

        [JsonProperty("portal")]
        [JsonPropertyName("portal")]
        public GSkill Portal { get; init; } = null!;

        [JsonProperty("power")]
        [JsonPropertyName("power")]
        public GSkill Power { get; init; } = null!;

        [JsonProperty("pure_eval")]
        [JsonPropertyName("pure_eval")]
        public GSkill PureEval { get; init; } = null!;

        [JsonProperty("purify")]
        [JsonPropertyName("purify")]
        public GSkill Purify { get; init; } = null!;

        [JsonProperty("quickpunch")]
        [JsonPropertyName("quickpunch")]
        public GSkill Quickpunch { get; init; } = null!;

        [JsonProperty("quickstab")]
        [JsonPropertyName("quickstab")]
        public GSkill Quickstab { get; init; } = null!;

        [JsonProperty("reflection")]
        [JsonPropertyName("reflection")]
        public GSkill Reflection { get; init; } = null!;

        [JsonProperty("regen_hp")]
        [JsonPropertyName("regen_hp")]
        public GSkill RegenHp { get; init; } = null!;

        [JsonProperty("regen_mp")]
        [JsonPropertyName("regen_mp")]
        public GSkill RegenMp { get; init; } = null!;

        [JsonProperty("revive")]
        [JsonPropertyName("revive")]
        public GSkill Revive { get; init; } = null!;

        [JsonProperty("rspeed")]
        [JsonPropertyName("rspeed")]
        public GSkill Rspeed { get; init; } = null!;

        [JsonProperty("scare")]
        [JsonPropertyName("scare")]
        public GSkill Scare { get; init; } = null!;

        [JsonProperty("selfheal")]
        [JsonPropertyName("selfheal")]
        public GSkill Selfheal { get; init; } = null!;

        [JsonProperty("self_healing")]
        [JsonPropertyName("self_healing")]
        public GSkill SelfHealing { get; init; } = null!;

        [JsonProperty("shadowstrike")]
        [JsonPropertyName("shadowstrike")]
        public GSkill Shadowstrike { get; init; } = null!;

        [JsonProperty("smash")]
        [JsonPropertyName("smash")]
        public GSkill Smash { get; init; } = null!;

        [JsonProperty("snippet")]
        [JsonPropertyName("snippet")]
        public GSkill Snippet { get; init; } = null!;

        [JsonProperty("snowball")]
        [JsonPropertyName("snowball")]
        public GSkill Snowball { get; init; } = null!;

        [JsonProperty("stack")]
        [JsonPropertyName("stack")]
        public GSkill Stack { get; init; } = null!;

        [JsonProperty("stomp")]
        [JsonPropertyName("stomp")]
        public GSkill Stomp { get; init; } = null!;

        [JsonProperty("stone")]
        [JsonPropertyName("stone")]
        public GSkill Stone { get; init; } = null!;

        [JsonProperty("stop")]
        [JsonPropertyName("stop")]
        public GSkill Stop { get; init; } = null!;

        [JsonProperty("supershot")]
        [JsonPropertyName("supershot")]
        public GSkill Supershot { get; init; } = null!;

        [JsonProperty("tangle")]
        [JsonPropertyName("tangle")]
        public GSkill Tangle { get; init; } = null!;

        [JsonProperty("taunt")]
        [JsonPropertyName("taunt")]
        public GSkill Taunt { get; init; } = null!;

        [JsonProperty("temporalsurge")]
        [JsonPropertyName("temporalsurge")]
        public GSkill Temporalsurge { get; init; } = null!;

        [JsonProperty("throw")]
        [JsonPropertyName("throw")]
        public GSkill Throw { get; init; } = null!;

        [JsonProperty("toggle_character")]
        [JsonPropertyName("toggle_character")]
        public GSkill ToggleCharacter { get; init; } = null!;

        [JsonProperty("toggle_code")]
        [JsonPropertyName("toggle_code")]
        public GSkill ToggleCode { get; init; } = null!;

        [JsonProperty("toggle_inventory")]
        [JsonPropertyName("toggle_inventory")]
        public GSkill ToggleInventory { get; init; } = null!;

        [JsonProperty("toggle_run_code")]
        [JsonPropertyName("toggle_run_code")]
        public GSkill ToggleRunCode { get; init; } = null!;

        [JsonProperty("toggle_stats")]
        [JsonPropertyName("toggle_stats")]
        public GSkill ToggleStats { get; init; } = null!;

        [JsonProperty("track")]
        [JsonPropertyName("track")]
        public GSkill Track { get; init; } = null!;

        [JsonProperty("travel")]
        [JsonPropertyName("travel")]
        public GSkill Travel { get; init; } = null!;

        [JsonProperty("use_hp")]
        [JsonPropertyName("use_hp")]
        public GSkill UseHp { get; init; } = null!;

        [JsonProperty("use_mp")]
        [JsonPropertyName("use_mp")]
        public GSkill UseMp { get; init; } = null!;

        [JsonProperty("use_town")]
        [JsonPropertyName("use_town")]
        public GSkill UseTown { get; init; } = null!;

        [JsonProperty("warcry")]
        [JsonPropertyName("warcry")]
        public GSkill Warcry { get; init; } = null!;

        [JsonProperty("warp")]
        [JsonPropertyName("warp")]
        public GSkill Warp { get; init; } = null!;

        [JsonProperty("warpstomp")]
        [JsonPropertyName("warpstomp")]
        public GSkill Warpstomp { get; init; } = null!;

        [JsonProperty("weakness_aura")]
        [JsonPropertyName("weakness_aura")]
        public GSkill WeaknessAura { get; init; } = null!;

        [JsonProperty("xpower")]
        [JsonPropertyName("xpower")]
        public GSkill Xpower { get; init; } = null!;

        [JsonProperty("zap")]
        [JsonPropertyName("zap")]
        public GSkill Zap { get; init; } = null!;

        [JsonProperty("zapperzap")]
        [JsonPropertyName("zapperzap")]
        public GSkill Zapperzap { get; init; } = null!;
    }
}