#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.NPCs;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class NPCsDatum : DatumBase<GNPC>
{
    [JsonPropertyName("antip2w")]
    public GNPC Antip2W { get; init; } = null!;

    [JsonPropertyName("appearance")]
    public GNPC Appearance { get; init; } = null!;

    [JsonPropertyName("armors")]
    public GNPC Armors { get; init; } = null!;

    [JsonPropertyName("basics")]
    public GNPC Basics { get; init; } = null!;

    [JsonPropertyName("bean")]
    public GNPC Bean { get; init; } = null!;

    [JsonPropertyName("bouncer")]
    public GNPC Bouncer { get; init; } = null!;

    [JsonPropertyName("citizen0")]
    public GNPC Citizen0 { get; init; } = null!;

    [JsonPropertyName("citizen1")]
    public GNPC Citizen1 { get; init; } = null!;

    [JsonPropertyName("citizen10")]
    public GNPC Citizen10 { get; init; } = null!;

    [JsonPropertyName("citizen11")]
    public GNPC Citizen11 { get; init; } = null!;

    [JsonPropertyName("citizen12")]
    public GNPC Citizen12 { get; init; } = null!;

    [JsonPropertyName("citizen13")]
    public GNPC Citizen13 { get; init; } = null!;

    [JsonPropertyName("citizen14")]
    public GNPC Citizen14 { get; init; } = null!;

    [JsonPropertyName("citizen15")]
    public GNPC Citizen15 { get; init; } = null!;

    [JsonPropertyName("citizen16")]
    public GNPC Citizen16 { get; init; } = null!;

    [JsonPropertyName("citizen2")]
    public GNPC Citizen2 { get; init; } = null!;

    [JsonPropertyName("citizen3")]
    public GNPC Citizen3 { get; init; } = null!;

    [JsonPropertyName("citizen4")]
    public GNPC Citizen4 { get; init; } = null!;

    [JsonPropertyName("citizen5")]
    public GNPC Citizen5 { get; init; } = null!;

    [JsonPropertyName("citizen6")]
    public GNPC Citizen6 { get; init; } = null!;

    [JsonPropertyName("citizen7")]
    public GNPC Citizen7 { get; init; } = null!;

    [JsonPropertyName("citizen8")]
    public GNPC Citizen8 { get; init; } = null!;

    [JsonPropertyName("citizen9")]
    public GNPC Citizen9 { get; init; } = null!;

    [JsonPropertyName("compound")]
    public GNPC Compound { get; init; } = null!;

    [JsonPropertyName("craftsman")]
    public GNPC Craftsman { get; init; } = null!;

    [JsonPropertyName("exchange")]
    public GNPC Exchange { get; init; } = null!;

    [JsonPropertyName("fancypots")]
    public GNPC Fancypots { get; init; } = null!;

    [JsonPropertyName("favors")]
    public GNPC Favors { get; init; } = null!;

    [JsonPropertyName("firstc")]
    public GNPC Firstc { get; init; } = null!;

    [JsonPropertyName("fisherman")]
    public GNPC Fisherman { get; init; } = null!;

    [JsonPropertyName("friendtokens")]
    public GNPC Friendtokens { get; init; } = null!;

    [JsonPropertyName("funtokens")]
    public GNPC Funtokens { get; init; } = null!;

    [JsonPropertyName("gemmerchant")]
    public GNPC Gemmerchant { get; init; } = null!;

    [JsonPropertyName("goldnpc")]
    public GNPC Goldnpc { get; init; } = null!;

    [JsonPropertyName("guard")]
    public GNPC Guard { get; init; } = null!;

    [JsonPropertyName("holo")]
    public GNPC Holo { get; init; } = null!;

    [JsonPropertyName("holo0")]
    public GNPC Holo0 { get; init; } = null!;

    [JsonPropertyName("holo1")]
    public GNPC Holo1 { get; init; } = null!;

    [JsonPropertyName("holo2")]
    public GNPC Holo2 { get; init; } = null!;

    [JsonPropertyName("holo3")]
    public GNPC Holo3 { get; init; } = null!;

    [JsonPropertyName("holo4")]
    public GNPC Holo4 { get; init; } = null!;

    [JsonPropertyName("holo5")]
    public GNPC Holo5 { get; init; } = null!;

    [JsonPropertyName("items0")]
    public GNPC Items0 { get; init; } = null!;

    [JsonPropertyName("items1")]
    public GNPC Items1 { get; init; } = null!;

    [JsonPropertyName("items10")]
    public GNPC Items10 { get; init; } = null!;

    [JsonPropertyName("items11")]
    public GNPC Items11 { get; init; } = null!;

    [JsonPropertyName("items12")]
    public GNPC Items12 { get; init; } = null!;

    [JsonPropertyName("items13")]
    public GNPC Items13 { get; init; } = null!;

    [JsonPropertyName("items14")]
    public GNPC Items14 { get; init; } = null!;

    [JsonPropertyName("items15")]
    public GNPC Items15 { get; init; } = null!;

    [JsonPropertyName("items16")]
    public GNPC Items16 { get; init; } = null!;

    [JsonPropertyName("items17")]
    public GNPC Items17 { get; init; } = null!;

    [JsonPropertyName("items18")]
    public GNPC Items18 { get; init; } = null!;

    [JsonPropertyName("items19")]
    public GNPC Items19 { get; init; } = null!;

    [JsonPropertyName("items2")]
    public GNPC Items2 { get; init; } = null!;

    [JsonPropertyName("items20")]
    public GNPC Items20 { get; init; } = null!;

    [JsonPropertyName("items21")]
    public GNPC Items21 { get; init; } = null!;

    [JsonPropertyName("items22")]
    public GNPC Items22 { get; init; } = null!;

    [JsonPropertyName("items23")]
    public GNPC Items23 { get; init; } = null!;

    [JsonPropertyName("items24")]
    public GNPC Items24 { get; init; } = null!;

    [JsonPropertyName("items25")]
    public GNPC Items25 { get; init; } = null!;

    [JsonPropertyName("items26")]
    public GNPC Items26 { get; init; } = null!;

    [JsonPropertyName("items27")]
    public GNPC Items27 { get; init; } = null!;

    [JsonPropertyName("items28")]
    public GNPC Items28 { get; init; } = null!;

    [JsonPropertyName("items29")]
    public GNPC Items29 { get; init; } = null!;

    [JsonPropertyName("items3")]
    public GNPC Items3 { get; init; } = null!;

    [JsonPropertyName("items30")]
    public GNPC Items30 { get; init; } = null!;

    [JsonPropertyName("items31")]
    public GNPC Items31 { get; init; } = null!;

    [JsonPropertyName("items32")]
    public GNPC Items32 { get; init; } = null!;

    [JsonPropertyName("items33")]
    public GNPC Items33 { get; init; } = null!;

    [JsonPropertyName("items34")]
    public GNPC Items34 { get; init; } = null!;

    [JsonPropertyName("items35")]
    public GNPC Items35 { get; init; } = null!;

    [JsonPropertyName("items36")]
    public GNPC Items36 { get; init; } = null!;

    [JsonPropertyName("items37")]
    public GNPC Items37 { get; init; } = null!;

    [JsonPropertyName("items38")]
    public GNPC Items38 { get; init; } = null!;

    [JsonPropertyName("items39")]
    public GNPC Items39 { get; init; } = null!;

    [JsonPropertyName("items4")]
    public GNPC Items4 { get; init; } = null!;

    [JsonPropertyName("items40")]
    public GNPC Items40 { get; init; } = null!;

    [JsonPropertyName("items41")]
    public GNPC Items41 { get; init; } = null!;

    [JsonPropertyName("items42")]
    public GNPC Items42 { get; init; } = null!;

    [JsonPropertyName("items43")]
    public GNPC Items43 { get; init; } = null!;

    [JsonPropertyName("items44")]
    public GNPC Items44 { get; init; } = null!;

    [JsonPropertyName("items45")]
    public GNPC Items45 { get; init; } = null!;

    [JsonPropertyName("items46")]
    public GNPC Items46 { get; init; } = null!;

    [JsonPropertyName("items47")]
    public GNPC Items47 { get; init; } = null!;

    [JsonPropertyName("items5")]
    public GNPC Items5 { get; init; } = null!;

    [JsonPropertyName("items6")]
    public GNPC Items6 { get; init; } = null!;

    [JsonPropertyName("items7")]
    public GNPC Items7 { get; init; } = null!;

    [JsonPropertyName("items8")]
    public GNPC Items8 { get; init; } = null!;

    [JsonPropertyName("items9")]
    public GNPC Items9 { get; init; } = null!;

    [JsonPropertyName("jailer")]
    public GNPC Jailer { get; init; } = null!;

    [JsonPropertyName("leathermerchant")]
    public GNPC Leathermerchant { get; init; } = null!;

    [JsonPropertyName("lichteaser")]
    public GNPC Lichteaser { get; init; } = null!;

    [JsonPropertyName("locksmith")]
    public GNPC Locksmith { get; init; } = null!;

    [JsonPropertyName("lostandfound")]
    public GNPC Lostandfound { get; init; } = null!;

    [JsonPropertyName("lotterylady")]
    public GNPC Lotterylady { get; init; } = null!;

    [JsonPropertyName("mcollector")]
    public GNPC Mcollector { get; init; } = null!;

    [JsonPropertyName("mistletoe")]
    public GNPC Mistletoe { get; init; } = null!;

    [JsonPropertyName("monsterhunter")]
    public GNPC Monsterhunter { get; init; } = null!;

    [JsonPropertyName("newupgrade")]
    public GNPC Newupgrade { get; init; } = null!;

    [JsonPropertyName("newyear_tree")]
    public GNPC NewyearTree { get; init; } = null!;

    [JsonPropertyName("ornaments")]
    public GNPC Ornaments { get; init; } = null!;

    [JsonPropertyName("pete")]
    public GNPC Pete { get; init; } = null!;

    [JsonPropertyName("pots")]
    public GNPC Pots { get; init; } = null!;

    [JsonPropertyName("premium")]
    public GNPC Premium { get; init; } = null!;

    [JsonPropertyName("princess")]
    public GNPC Princess { get; init; } = null!;

    [JsonPropertyName("pvp")]
    public GNPC Pvp { get; init; } = null!;

    [JsonPropertyName("pvpblocker")]
    public GNPC Pvpblocker { get; init; } = null!;

    [JsonPropertyName("pvptokens")]
    public GNPC Pvptokens { get; init; } = null!;

    [JsonPropertyName("pwincess")]
    public GNPC Pwincess { get; init; } = null!;

    [JsonPropertyName("rewards")]
    public GNPC Rewards { get; init; } = null!;

    [JsonPropertyName("santa")]
    public GNPC Santa { get; init; } = null!;

    [JsonPropertyName("scrolls")]
    public GNPC Scrolls { get; init; } = null!;

    [JsonPropertyName("scrollsmith")]
    public GNPC Scrollsmith { get; init; } = null!;

    [JsonPropertyName("secondhands")]
    public GNPC Secondhands { get; init; } = null!;

    [JsonPropertyName("shellsguy")]
    public GNPC Shellsguy { get; init; } = null!;

    [JsonPropertyName("ship")]
    public GNPC Ship { get; init; } = null!;

    [JsonPropertyName("shrine")]
    public GNPC Shrine { get; init; } = null!;

    [JsonPropertyName("standmerchant")]
    public GNPC Standmerchant { get; init; } = null!;

    [JsonPropertyName("tavern")]
    public GNPC Tavern { get; init; } = null!;

    [JsonPropertyName("tbartender")]
    public GNPC Tbartender { get; init; } = null!;

    [JsonPropertyName("thief")]
    public GNPC Thief { get; init; } = null!;

    [JsonPropertyName("transporter")]
    public GNPC Transporter { get; init; } = null!;

    [JsonPropertyName("wbartender")]
    public GNPC Wbartender { get; init; } = null!;

    [JsonPropertyName("weapons")]
    public GNPC Weapons { get; init; } = null!;

    [JsonPropertyName("witch")]
    public GNPC Witch { get; init; } = null!;

    [JsonPropertyName("wizardrepeater")]
    public GNPC Wizardrepeater { get; init; } = null!;

    [JsonPropertyName("wnpc")]
    public GNPC Wnpc { get; init; } = null!;
}