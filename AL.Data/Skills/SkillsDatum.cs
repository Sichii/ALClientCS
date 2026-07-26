#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Skills;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class SkillsDatum : DatumBase<GSkill>
{
    [JsonPropertyName("3shot")]
    public GSkill _3Shot { get; init; } = null!;

    [JsonPropertyName("4fingers")]
    public GSkill _4Fingers { get; init; } = null!;

    [JsonPropertyName("5shot")]
    public GSkill _5Shot { get; init; } = null!;

    [JsonPropertyName("absorb")]
    public GSkill Absorb { get; init; } = null!;

    [JsonPropertyName("agitate")]
    public GSkill Agitate { get; init; } = null!;

    [JsonPropertyName("alchemy")]
    public GSkill Alchemy { get; init; } = null!;

    [JsonPropertyName("anger")]
    public GSkill Anger { get; init; } = null!;

    [JsonPropertyName("attack")]
    public GSkill Attack { get; init; } = null!;

    [JsonPropertyName("blink")]
    public GSkill Blink { get; init; } = null!;

    [JsonPropertyName("burst")]
    public GSkill Burst { get; init; } = null!;

    [JsonPropertyName("cburst")]
    public GSkill Cburst { get; init; } = null!;

    [JsonPropertyName("charge")]
    public GSkill Charge { get; init; } = null!;

    [JsonPropertyName("charm")]
    public GSkill Charm { get; init; } = null!;

    [JsonPropertyName("cleave")]
    public GSkill Cleave { get; init; } = null!;

    [JsonPropertyName("curse")]
    public GSkill Curse { get; init; } = null!;

    [JsonPropertyName("curse_aura")]
    public GSkill CurseAura { get; init; } = null!;

    [JsonPropertyName("dampening_aura")]
    public GSkill DampeningAura { get; init; } = null!;

    [JsonPropertyName("darkblessing")]
    public GSkill Darkblessing { get; init; } = null!;

    [JsonPropertyName("dash")]
    public GSkill Dash { get; init; } = null!;

    [JsonPropertyName("deepfreeze")]
    public GSkill Deepfreeze { get; init; } = null!;

    [JsonPropertyName("emotion")]
    public GSkill Emotion { get; init; } = null!;

    [JsonPropertyName("energize")]
    public GSkill Energize { get; init; } = null!;

    [JsonPropertyName("entangle")]
    public GSkill Entangle { get; init; } = null!;

    [JsonPropertyName("esc")]
    public GSkill Esc { get; init; } = null!;

    [JsonPropertyName("fireball")]
    public GSkill Fireball { get; init; } = null!;

    [JsonPropertyName("fishing")]
    public GSkill Fishing { get; init; } = null!;

    [JsonPropertyName("frostball")]
    public GSkill Frostball { get; init; } = null!;

    [JsonPropertyName("gm")]
    public GSkill Gm { get; init; } = null!;

    [JsonPropertyName("hardshell")]
    public GSkill Hardshell { get; init; } = null!;

    [JsonPropertyName("heal")]
    public GSkill Heal { get; init; } = null!;

    [JsonPropertyName("healing")]
    public GSkill Healing { get; init; } = null!;

    [JsonPropertyName("huntersmark")]
    public GSkill Huntersmark { get; init; } = null!;

    [JsonPropertyName("interact")]
    public GSkill Interact { get; init; } = null!;

    [JsonPropertyName("invis")]
    public GSkill Invis { get; init; } = null!;

    [JsonPropertyName("light")]
    public GSkill Light { get; init; } = null!;

    [JsonPropertyName("magiport")]
    public GSkill Magiport { get; init; } = null!;

    [JsonPropertyName("massexchange")]
    public GSkill Massexchange { get; init; } = null!;

    [JsonPropertyName("massexchangepp")]
    public GSkill Massexchangepp { get; init; } = null!;

    [JsonPropertyName("massproduction")]
    public GSkill Massproduction { get; init; } = null!;

    [JsonPropertyName("massproductionpp")]
    public GSkill Massproductionpp { get; init; } = null!;

    [JsonPropertyName("mcourage")]
    public GSkill Mcourage { get; init; } = null!;

    [JsonPropertyName("mentalburst")]
    public GSkill Mentalburst { get; init; } = null!;

    [JsonPropertyName("mfrenzy")]
    public GSkill Mfrenzy { get; init; } = null!;

    [JsonPropertyName("mining")]
    public GSkill Mining { get; init; } = null!;

    [JsonPropertyName("mlight")]
    public GSkill Mlight { get; init; } = null!;

    [JsonPropertyName("mluck")]
    public GSkill Mluck { get; init; } = null!;

    [JsonPropertyName("move_down")]
    public GSkill MoveDown { get; init; } = null!;

    [JsonPropertyName("move_left")]
    public GSkill MoveLeft { get; init; } = null!;

    [JsonPropertyName("move_right")]
    public GSkill MoveRight { get; init; } = null!;

    [JsonPropertyName("move_up")]
    public GSkill MoveUp { get; init; } = null!;

    [JsonPropertyName("mshield")]
    public GSkill Mshield { get; init; } = null!;

    [JsonPropertyName("mtangle")]
    public GSkill Mtangle { get; init; } = null!;

    [JsonPropertyName("multi_burn")]
    public GSkill MultiBurn { get; init; } = null!;

    [JsonPropertyName("multi_freeze")]
    public GSkill MultiFreeze { get; init; } = null!;

    [JsonPropertyName("open_snippet")]
    public GSkill OpenSnippet { get; init; } = null!;

    [JsonPropertyName("partyheal")]
    public GSkill Partyheal { get; init; } = null!;

    [JsonPropertyName("pcoat")]
    public GSkill Pcoat { get; init; } = null!;

    [JsonPropertyName("phaseout")]
    public GSkill Phaseout { get; init; } = null!;

    [JsonPropertyName("pickpocket")]
    public GSkill Pickpocket { get; init; } = null!;

    [JsonPropertyName("piercingshot")]
    public GSkill Piercingshot { get; init; } = null!;

    [JsonPropertyName("poisonarrow")]
    public GSkill Poisonarrow { get; init; } = null!;

    [JsonPropertyName("portal")]
    public GSkill Portal { get; init; } = null!;

    [JsonPropertyName("power")]
    public GSkill Power { get; init; } = null!;

    [JsonPropertyName("pure_eval")]
    public GSkill PureEval { get; init; } = null!;

    [JsonPropertyName("purify")]
    public GSkill Purify { get; init; } = null!;

    [JsonPropertyName("quickpunch")]
    public GSkill Quickpunch { get; init; } = null!;

    [JsonPropertyName("quickstab")]
    public GSkill Quickstab { get; init; } = null!;

    [JsonPropertyName("reflection")]
    public GSkill Reflection { get; init; } = null!;

    [JsonPropertyName("regen_hp")]
    public GSkill RegenHp { get; init; } = null!;

    [JsonPropertyName("regen_mp")]
    public GSkill RegenMp { get; init; } = null!;

    [JsonPropertyName("revive")]
    public GSkill Revive { get; init; } = null!;

    [JsonPropertyName("rspeed")]
    public GSkill Rspeed { get; init; } = null!;

    [JsonPropertyName("scare")]
    public GSkill Scare { get; init; } = null!;

    [JsonPropertyName("selfheal")]
    public GSkill Selfheal { get; init; } = null!;

    [JsonPropertyName("self_healing")]
    public GSkill SelfHealing { get; init; } = null!;

    [JsonPropertyName("shadowstrike")]
    public GSkill Shadowstrike { get; init; } = null!;

    [JsonPropertyName("smash")]
    public GSkill Smash { get; init; } = null!;

    [JsonPropertyName("snippet")]
    public GSkill Snippet { get; init; } = null!;

    [JsonPropertyName("snowball")]
    public GSkill Snowball { get; init; } = null!;

    [JsonPropertyName("stack")]
    public GSkill Stack { get; init; } = null!;

    [JsonPropertyName("stomp")]
    public GSkill Stomp { get; init; } = null!;

    [JsonPropertyName("stone")]
    public GSkill Stone { get; init; } = null!;

    [JsonPropertyName("stop")]
    public GSkill Stop { get; init; } = null!;

    [JsonPropertyName("supershot")]
    public GSkill Supershot { get; init; } = null!;

    [JsonPropertyName("tangle")]
    public GSkill Tangle { get; init; } = null!;

    [JsonPropertyName("taunt")]
    public GSkill Taunt { get; init; } = null!;

    [JsonPropertyName("temporalsurge")]
    public GSkill Temporalsurge { get; init; } = null!;

    [JsonPropertyName("throw")]
    public GSkill Throw { get; init; } = null!;

    [JsonPropertyName("toggle_character")]
    public GSkill ToggleCharacter { get; init; } = null!;

    [JsonPropertyName("toggle_code")]
    public GSkill ToggleCode { get; init; } = null!;

    [JsonPropertyName("toggle_inventory")]
    public GSkill ToggleInventory { get; init; } = null!;

    [JsonPropertyName("toggle_run_code")]
    public GSkill ToggleRunCode { get; init; } = null!;

    [JsonPropertyName("toggle_stats")]
    public GSkill ToggleStats { get; init; } = null!;

    [JsonPropertyName("track")]
    public GSkill Track { get; init; } = null!;

    [JsonPropertyName("travel")]
    public GSkill Travel { get; init; } = null!;

    [JsonPropertyName("use_hp")]
    public GSkill UseHp { get; init; } = null!;

    [JsonPropertyName("use_mp")]
    public GSkill UseMp { get; init; } = null!;

    [JsonPropertyName("use_town")]
    public GSkill UseTown { get; init; } = null!;

    [JsonPropertyName("warcry")]
    public GSkill Warcry { get; init; } = null!;

    [JsonPropertyName("warp")]
    public GSkill Warp { get; init; } = null!;

    [JsonPropertyName("warpstomp")]
    public GSkill Warpstomp { get; init; } = null!;

    [JsonPropertyName("weakness_aura")]
    public GSkill WeaknessAura { get; init; } = null!;

    [JsonPropertyName("xpower")]
    public GSkill Xpower { get; init; } = null!;

    [JsonPropertyName("zap")]
    public GSkill Zap { get; init; } = null!;

    [JsonPropertyName("zapperzap")]
    public GSkill Zapperzap { get; init; } = null!;
}