#region
using System.Text.Json.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.NPCs
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GNPC>))]
    public class NPCsDatum : DatumBase<GNPC>
    {
        [JsonProperty("antip2w")]
        [JsonPropertyName("antip2w")]
        public GNPC Antip2W { get; init; } = null!;

        [JsonProperty("appearance")]
        [JsonPropertyName("appearance")]
        public GNPC Appearance { get; init; } = null!;

        [JsonProperty("armors")]
        [JsonPropertyName("armors")]
        public GNPC Armors { get; init; } = null!;

        [JsonProperty("basics")]
        [JsonPropertyName("basics")]
        public GNPC Basics { get; init; } = null!;

        [JsonProperty("bean")]
        [JsonPropertyName("bean")]
        public GNPC Bean { get; init; } = null!;

        [JsonProperty("bouncer")]
        [JsonPropertyName("bouncer")]
        public GNPC Bouncer { get; init; } = null!;

        [JsonProperty("citizen0")]
        [JsonPropertyName("citizen0")]
        public GNPC Citizen0 { get; init; } = null!;

        [JsonProperty("citizen1")]
        [JsonPropertyName("citizen1")]
        public GNPC Citizen1 { get; init; } = null!;

        [JsonProperty("citizen10")]
        [JsonPropertyName("citizen10")]
        public GNPC Citizen10 { get; init; } = null!;

        [JsonProperty("citizen11")]
        [JsonPropertyName("citizen11")]
        public GNPC Citizen11 { get; init; } = null!;

        [JsonProperty("citizen12")]
        [JsonPropertyName("citizen12")]
        public GNPC Citizen12 { get; init; } = null!;

        [JsonProperty("citizen13")]
        [JsonPropertyName("citizen13")]
        public GNPC Citizen13 { get; init; } = null!;

        [JsonProperty("citizen14")]
        [JsonPropertyName("citizen14")]
        public GNPC Citizen14 { get; init; } = null!;

        [JsonProperty("citizen15")]
        [JsonPropertyName("citizen15")]
        public GNPC Citizen15 { get; init; } = null!;

        [JsonProperty("citizen16")]
        [JsonPropertyName("citizen16")]
        public GNPC Citizen16 { get; init; } = null!;

        [JsonProperty("citizen2")]
        [JsonPropertyName("citizen2")]
        public GNPC Citizen2 { get; init; } = null!;

        [JsonProperty("citizen3")]
        [JsonPropertyName("citizen3")]
        public GNPC Citizen3 { get; init; } = null!;

        [JsonProperty("citizen4")]
        [JsonPropertyName("citizen4")]
        public GNPC Citizen4 { get; init; } = null!;

        [JsonProperty("citizen5")]
        [JsonPropertyName("citizen5")]
        public GNPC Citizen5 { get; init; } = null!;

        [JsonProperty("citizen6")]
        [JsonPropertyName("citizen6")]
        public GNPC Citizen6 { get; init; } = null!;

        [JsonProperty("citizen7")]
        [JsonPropertyName("citizen7")]
        public GNPC Citizen7 { get; init; } = null!;

        [JsonProperty("citizen8")]
        [JsonPropertyName("citizen8")]
        public GNPC Citizen8 { get; init; } = null!;

        [JsonProperty("citizen9")]
        [JsonPropertyName("citizen9")]
        public GNPC Citizen9 { get; init; } = null!;

        [JsonProperty("compound")]
        [JsonPropertyName("compound")]
        public GNPC Compound { get; init; } = null!;

        [JsonProperty("craftsman")]
        [JsonPropertyName("craftsman")]
        public GNPC Craftsman { get; init; } = null!;

        [JsonProperty("exchange")]
        [JsonPropertyName("exchange")]
        public GNPC Exchange { get; init; } = null!;

        [JsonProperty("fancypots")]
        [JsonPropertyName("fancypots")]
        public GNPC Fancypots { get; init; } = null!;

        [JsonProperty("favors")]
        [JsonPropertyName("favors")]
        public GNPC Favors { get; init; } = null!;

        [JsonProperty("firstc")]
        [JsonPropertyName("firstc")]
        public GNPC Firstc { get; init; } = null!;

        [JsonProperty("fisherman")]
        [JsonPropertyName("fisherman")]
        public GNPC Fisherman { get; init; } = null!;

        [JsonProperty("friendtokens")]
        [JsonPropertyName("friendtokens")]
        public GNPC Friendtokens { get; init; } = null!;

        [JsonProperty("funtokens")]
        [JsonPropertyName("funtokens")]
        public GNPC Funtokens { get; init; } = null!;

        [JsonProperty("gemmerchant")]
        [JsonPropertyName("gemmerchant")]
        public GNPC Gemmerchant { get; init; } = null!;

        [JsonProperty("goldnpc")]
        [JsonPropertyName("goldnpc")]
        public GNPC Goldnpc { get; init; } = null!;

        [JsonProperty("guard")]
        [JsonPropertyName("guard")]
        public GNPC Guard { get; init; } = null!;

        [JsonProperty("holo")]
        [JsonPropertyName("holo")]
        public GNPC Holo { get; init; } = null!;

        [JsonProperty("holo0")]
        [JsonPropertyName("holo0")]
        public GNPC Holo0 { get; init; } = null!;

        [JsonProperty("holo1")]
        [JsonPropertyName("holo1")]
        public GNPC Holo1 { get; init; } = null!;

        [JsonProperty("holo2")]
        [JsonPropertyName("holo2")]
        public GNPC Holo2 { get; init; } = null!;

        [JsonProperty("holo3")]
        [JsonPropertyName("holo3")]
        public GNPC Holo3 { get; init; } = null!;

        [JsonProperty("holo4")]
        [JsonPropertyName("holo4")]
        public GNPC Holo4 { get; init; } = null!;

        [JsonProperty("holo5")]
        [JsonPropertyName("holo5")]
        public GNPC Holo5 { get; init; } = null!;

        [JsonProperty("items0")]
        [JsonPropertyName("items0")]
        public GNPC Items0 { get; init; } = null!;

        [JsonProperty("items1")]
        [JsonPropertyName("items1")]
        public GNPC Items1 { get; init; } = null!;

        [JsonProperty("items10")]
        [JsonPropertyName("items10")]
        public GNPC Items10 { get; init; } = null!;

        [JsonProperty("items11")]
        [JsonPropertyName("items11")]
        public GNPC Items11 { get; init; } = null!;

        [JsonProperty("items12")]
        [JsonPropertyName("items12")]
        public GNPC Items12 { get; init; } = null!;

        [JsonProperty("items13")]
        [JsonPropertyName("items13")]
        public GNPC Items13 { get; init; } = null!;

        [JsonProperty("items14")]
        [JsonPropertyName("items14")]
        public GNPC Items14 { get; init; } = null!;

        [JsonProperty("items15")]
        [JsonPropertyName("items15")]
        public GNPC Items15 { get; init; } = null!;

        [JsonProperty("items16")]
        [JsonPropertyName("items16")]
        public GNPC Items16 { get; init; } = null!;

        [JsonProperty("items17")]
        [JsonPropertyName("items17")]
        public GNPC Items17 { get; init; } = null!;

        [JsonProperty("items18")]
        [JsonPropertyName("items18")]
        public GNPC Items18 { get; init; } = null!;

        [JsonProperty("items19")]
        [JsonPropertyName("items19")]
        public GNPC Items19 { get; init; } = null!;

        [JsonProperty("items2")]
        [JsonPropertyName("items2")]
        public GNPC Items2 { get; init; } = null!;

        [JsonProperty("items20")]
        [JsonPropertyName("items20")]
        public GNPC Items20 { get; init; } = null!;

        [JsonProperty("items21")]
        [JsonPropertyName("items21")]
        public GNPC Items21 { get; init; } = null!;

        [JsonProperty("items22")]
        [JsonPropertyName("items22")]
        public GNPC Items22 { get; init; } = null!;

        [JsonProperty("items23")]
        [JsonPropertyName("items23")]
        public GNPC Items23 { get; init; } = null!;

        [JsonProperty("items24")]
        [JsonPropertyName("items24")]
        public GNPC Items24 { get; init; } = null!;

        [JsonProperty("items25")]
        [JsonPropertyName("items25")]
        public GNPC Items25 { get; init; } = null!;

        [JsonProperty("items26")]
        [JsonPropertyName("items26")]
        public GNPC Items26 { get; init; } = null!;

        [JsonProperty("items27")]
        [JsonPropertyName("items27")]
        public GNPC Items27 { get; init; } = null!;

        [JsonProperty("items28")]
        [JsonPropertyName("items28")]
        public GNPC Items28 { get; init; } = null!;

        [JsonProperty("items29")]
        [JsonPropertyName("items29")]
        public GNPC Items29 { get; init; } = null!;

        [JsonProperty("items3")]
        [JsonPropertyName("items3")]
        public GNPC Items3 { get; init; } = null!;

        [JsonProperty("items30")]
        [JsonPropertyName("items30")]
        public GNPC Items30 { get; init; } = null!;

        [JsonProperty("items31")]
        [JsonPropertyName("items31")]
        public GNPC Items31 { get; init; } = null!;

        [JsonProperty("items32")]
        [JsonPropertyName("items32")]
        public GNPC Items32 { get; init; } = null!;

        [JsonProperty("items33")]
        [JsonPropertyName("items33")]
        public GNPC Items33 { get; init; } = null!;

        [JsonProperty("items34")]
        [JsonPropertyName("items34")]
        public GNPC Items34 { get; init; } = null!;

        [JsonProperty("items35")]
        [JsonPropertyName("items35")]
        public GNPC Items35 { get; init; } = null!;

        [JsonProperty("items36")]
        [JsonPropertyName("items36")]
        public GNPC Items36 { get; init; } = null!;

        [JsonProperty("items37")]
        [JsonPropertyName("items37")]
        public GNPC Items37 { get; init; } = null!;

        [JsonProperty("items38")]
        [JsonPropertyName("items38")]
        public GNPC Items38 { get; init; } = null!;

        [JsonProperty("items39")]
        [JsonPropertyName("items39")]
        public GNPC Items39 { get; init; } = null!;

        [JsonProperty("items4")]
        [JsonPropertyName("items4")]
        public GNPC Items4 { get; init; } = null!;

        [JsonProperty("items40")]
        [JsonPropertyName("items40")]
        public GNPC Items40 { get; init; } = null!;

        [JsonProperty("items41")]
        [JsonPropertyName("items41")]
        public GNPC Items41 { get; init; } = null!;

        [JsonProperty("items42")]
        [JsonPropertyName("items42")]
        public GNPC Items42 { get; init; } = null!;

        [JsonProperty("items43")]
        [JsonPropertyName("items43")]
        public GNPC Items43 { get; init; } = null!;

        [JsonProperty("items44")]
        [JsonPropertyName("items44")]
        public GNPC Items44 { get; init; } = null!;

        [JsonProperty("items45")]
        [JsonPropertyName("items45")]
        public GNPC Items45 { get; init; } = null!;

        [JsonProperty("items46")]
        [JsonPropertyName("items46")]
        public GNPC Items46 { get; init; } = null!;

        [JsonProperty("items47")]
        [JsonPropertyName("items47")]
        public GNPC Items47 { get; init; } = null!;

        [JsonProperty("items5")]
        [JsonPropertyName("items5")]
        public GNPC Items5 { get; init; } = null!;

        [JsonProperty("items6")]
        [JsonPropertyName("items6")]
        public GNPC Items6 { get; init; } = null!;

        [JsonProperty("items7")]
        [JsonPropertyName("items7")]
        public GNPC Items7 { get; init; } = null!;

        [JsonProperty("items8")]
        [JsonPropertyName("items8")]
        public GNPC Items8 { get; init; } = null!;

        [JsonProperty("items9")]
        [JsonPropertyName("items9")]
        public GNPC Items9 { get; init; } = null!;

        [JsonProperty("jailer")]
        [JsonPropertyName("jailer")]
        public GNPC Jailer { get; init; } = null!;

        [JsonProperty("leathermerchant")]
        [JsonPropertyName("leathermerchant")]
        public GNPC Leathermerchant { get; init; } = null!;

        [JsonProperty("lichteaser")]
        [JsonPropertyName("lichteaser")]
        public GNPC Lichteaser { get; init; } = null!;

        [JsonProperty("locksmith")]
        [JsonPropertyName("locksmith")]
        public GNPC Locksmith { get; init; } = null!;

        [JsonProperty("lostandfound")]
        [JsonPropertyName("lostandfound")]
        public GNPC Lostandfound { get; init; } = null!;

        [JsonProperty("lotterylady")]
        [JsonPropertyName("lotterylady")]
        public GNPC Lotterylady { get; init; } = null!;

        [JsonProperty("mcollector")]
        [JsonPropertyName("mcollector")]
        public GNPC Mcollector { get; init; } = null!;

        [JsonProperty("mistletoe")]
        [JsonPropertyName("mistletoe")]
        public GNPC Mistletoe { get; init; } = null!;

        [JsonProperty("monsterhunter")]
        [JsonPropertyName("monsterhunter")]
        public GNPC Monsterhunter { get; init; } = null!;

        [JsonProperty("newupgrade")]
        [JsonPropertyName("newupgrade")]
        public GNPC Newupgrade { get; init; } = null!;

        [JsonProperty("newyear_tree")]
        [JsonPropertyName("newyear_tree")]
        public GNPC NewyearTree { get; init; } = null!;

        [JsonProperty("ornaments")]
        [JsonPropertyName("ornaments")]
        public GNPC Ornaments { get; init; } = null!;

        [JsonProperty("pete")]
        [JsonPropertyName("pete")]
        public GNPC Pete { get; init; } = null!;

        [JsonProperty("pots")]
        [JsonPropertyName("pots")]
        public GNPC Pots { get; init; } = null!;

        [JsonProperty("premium")]
        [JsonPropertyName("premium")]
        public GNPC Premium { get; init; } = null!;

        [JsonProperty("princess")]
        [JsonPropertyName("princess")]
        public GNPC Princess { get; init; } = null!;

        [JsonProperty("pvp")]
        [JsonPropertyName("pvp")]
        public GNPC Pvp { get; init; } = null!;

        [JsonProperty("pvpblocker")]
        [JsonPropertyName("pvpblocker")]
        public GNPC Pvpblocker { get; init; } = null!;

        [JsonProperty("pvptokens")]
        [JsonPropertyName("pvptokens")]
        public GNPC Pvptokens { get; init; } = null!;

        [JsonProperty("pwincess")]
        [JsonPropertyName("pwincess")]
        public GNPC Pwincess { get; init; } = null!;

        [JsonProperty("rewards")]
        [JsonPropertyName("rewards")]
        public GNPC Rewards { get; init; } = null!;

        [JsonProperty("santa")]
        [JsonPropertyName("santa")]
        public GNPC Santa { get; init; } = null!;

        [JsonProperty("scrolls")]
        [JsonPropertyName("scrolls")]
        public GNPC Scrolls { get; init; } = null!;

        [JsonProperty("scrollsmith")]
        [JsonPropertyName("scrollsmith")]
        public GNPC Scrollsmith { get; init; } = null!;

        [JsonProperty("secondhands")]
        [JsonPropertyName("secondhands")]
        public GNPC Secondhands { get; init; } = null!;

        [JsonProperty("shellsguy")]
        [JsonPropertyName("shellsguy")]
        public GNPC Shellsguy { get; init; } = null!;

        [JsonProperty("ship")]
        [JsonPropertyName("ship")]
        public GNPC Ship { get; init; } = null!;

        [JsonProperty("shrine")]
        [JsonPropertyName("shrine")]
        public GNPC Shrine { get; init; } = null!;

        [JsonProperty("standmerchant")]
        [JsonPropertyName("standmerchant")]
        public GNPC Standmerchant { get; init; } = null!;

        [JsonProperty("tavern")]
        [JsonPropertyName("tavern")]
        public GNPC Tavern { get; init; } = null!;

        [JsonProperty("tbartender")]
        [JsonPropertyName("tbartender")]
        public GNPC Tbartender { get; init; } = null!;

        [JsonProperty("thief")]
        [JsonPropertyName("thief")]
        public GNPC Thief { get; init; } = null!;

        [JsonProperty("transporter")]
        [JsonPropertyName("transporter")]
        public GNPC Transporter { get; init; } = null!;

        [JsonProperty("wbartender")]
        [JsonPropertyName("wbartender")]
        public GNPC Wbartender { get; init; } = null!;

        [JsonProperty("weapons")]
        [JsonPropertyName("weapons")]
        public GNPC Weapons { get; init; } = null!;

        [JsonProperty("witch")]
        [JsonPropertyName("witch")]
        public GNPC Witch { get; init; } = null!;

        [JsonProperty("wizardrepeater")]
        [JsonPropertyName("wizardrepeater")]
        public GNPC Wizardrepeater { get; init; } = null!;

        [JsonProperty("wnpc")]
        [JsonPropertyName("wnpc")]
        public GNPC Wnpc { get; init; } = null!;
    }
}