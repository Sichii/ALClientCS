#region
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
        public GNPC Antip2W { get; init; } = null!;

        [JsonProperty("appearance")]
        public GNPC Appearance { get; init; } = null!;

        [JsonProperty("armors")]
        public GNPC Armors { get; init; } = null!;

        [JsonProperty("basics")]
        public GNPC Basics { get; init; } = null!;

        [JsonProperty("bean")]
        public GNPC Bean { get; init; } = null!;

        [JsonProperty("bouncer")]
        public GNPC Bouncer { get; init; } = null!;

        [JsonProperty("citizen0")]
        public GNPC Citizen0 { get; init; } = null!;

        [JsonProperty("citizen1")]
        public GNPC Citizen1 { get; init; } = null!;

        [JsonProperty("citizen10")]
        public GNPC Citizen10 { get; init; } = null!;

        [JsonProperty("citizen11")]
        public GNPC Citizen11 { get; init; } = null!;

        [JsonProperty("citizen12")]
        public GNPC Citizen12 { get; init; } = null!;

        [JsonProperty("citizen13")]
        public GNPC Citizen13 { get; init; } = null!;

        [JsonProperty("citizen14")]
        public GNPC Citizen14 { get; init; } = null!;

        [JsonProperty("citizen15")]
        public GNPC Citizen15 { get; init; } = null!;

        [JsonProperty("citizen16")]
        public GNPC Citizen16 { get; init; } = null!;

        [JsonProperty("citizen2")]
        public GNPC Citizen2 { get; init; } = null!;

        [JsonProperty("citizen3")]
        public GNPC Citizen3 { get; init; } = null!;

        [JsonProperty("citizen4")]
        public GNPC Citizen4 { get; init; } = null!;

        [JsonProperty("citizen5")]
        public GNPC Citizen5 { get; init; } = null!;

        [JsonProperty("citizen6")]
        public GNPC Citizen6 { get; init; } = null!;

        [JsonProperty("citizen7")]
        public GNPC Citizen7 { get; init; } = null!;

        [JsonProperty("citizen8")]
        public GNPC Citizen8 { get; init; } = null!;

        [JsonProperty("citizen9")]
        public GNPC Citizen9 { get; init; } = null!;

        [JsonProperty("compound")]
        public GNPC Compound { get; init; } = null!;

        [JsonProperty("craftsman")]
        public GNPC Craftsman { get; init; } = null!;

        [JsonProperty("exchange")]
        public GNPC Exchange { get; init; } = null!;

        [JsonProperty("fancypots")]
        public GNPC Fancypots { get; init; } = null!;

        [JsonProperty("firstc")]
        public GNPC Firstc { get; init; } = null!;

        [JsonProperty("fisherman")]
        public GNPC Fisherman { get; init; } = null!;

        [JsonProperty("friendtokens")]
        public GNPC Friendtokens { get; init; } = null!;

        [JsonProperty("funtokens")]
        public GNPC Funtokens { get; init; } = null!;

        [JsonProperty("gemmerchant")]
        public GNPC Gemmerchant { get; init; } = null!;

        [JsonProperty("goldnpc")]
        public GNPC Goldnpc { get; init; } = null!;

        [JsonProperty("guard")]
        public GNPC Guard { get; init; } = null!;

        [JsonProperty("holo")]
        public GNPC Holo { get; init; } = null!;

        [JsonProperty("holo0")]
        public GNPC Holo0 { get; init; } = null!;

        [JsonProperty("holo1")]
        public GNPC Holo1 { get; init; } = null!;

        [JsonProperty("holo2")]
        public GNPC Holo2 { get; init; } = null!;

        [JsonProperty("holo3")]
        public GNPC Holo3 { get; init; } = null!;

        [JsonProperty("holo4")]
        public GNPC Holo4 { get; init; } = null!;

        [JsonProperty("holo5")]
        public GNPC Holo5 { get; init; } = null!;

        [JsonProperty("items0")]
        public GNPC Items0 { get; init; } = null!;

        [JsonProperty("items1")]
        public GNPC Items1 { get; init; } = null!;

        [JsonProperty("items10")]
        public GNPC Items10 { get; init; } = null!;

        [JsonProperty("items11")]
        public GNPC Items11 { get; init; } = null!;

        [JsonProperty("items12")]
        public GNPC Items12 { get; init; } = null!;

        [JsonProperty("items13")]
        public GNPC Items13 { get; init; } = null!;

        [JsonProperty("items14")]
        public GNPC Items14 { get; init; } = null!;

        [JsonProperty("items15")]
        public GNPC Items15 { get; init; } = null!;

        [JsonProperty("items16")]
        public GNPC Items16 { get; init; } = null!;

        [JsonProperty("items17")]
        public GNPC Items17 { get; init; } = null!;

        [JsonProperty("items18")]
        public GNPC Items18 { get; init; } = null!;

        [JsonProperty("items19")]
        public GNPC Items19 { get; init; } = null!;

        [JsonProperty("items2")]
        public GNPC Items2 { get; init; } = null!;

        [JsonProperty("items20")]
        public GNPC Items20 { get; init; } = null!;

        [JsonProperty("items21")]
        public GNPC Items21 { get; init; } = null!;

        [JsonProperty("items22")]
        public GNPC Items22 { get; init; } = null!;

        [JsonProperty("items23")]
        public GNPC Items23 { get; init; } = null!;

        [JsonProperty("items24")]
        public GNPC Items24 { get; init; } = null!;

        [JsonProperty("items25")]
        public GNPC Items25 { get; init; } = null!;

        [JsonProperty("items26")]
        public GNPC Items26 { get; init; } = null!;

        [JsonProperty("items27")]
        public GNPC Items27 { get; init; } = null!;

        [JsonProperty("items28")]
        public GNPC Items28 { get; init; } = null!;

        [JsonProperty("items29")]
        public GNPC Items29 { get; init; } = null!;

        [JsonProperty("items3")]
        public GNPC Items3 { get; init; } = null!;

        [JsonProperty("items30")]
        public GNPC Items30 { get; init; } = null!;

        [JsonProperty("items31")]
        public GNPC Items31 { get; init; } = null!;

        [JsonProperty("items32")]
        public GNPC Items32 { get; init; } = null!;

        [JsonProperty("items33")]
        public GNPC Items33 { get; init; } = null!;

        [JsonProperty("items34")]
        public GNPC Items34 { get; init; } = null!;

        [JsonProperty("items35")]
        public GNPC Items35 { get; init; } = null!;

        [JsonProperty("items36")]
        public GNPC Items36 { get; init; } = null!;

        [JsonProperty("items37")]
        public GNPC Items37 { get; init; } = null!;

        [JsonProperty("items38")]
        public GNPC Items38 { get; init; } = null!;

        [JsonProperty("items39")]
        public GNPC Items39 { get; init; } = null!;

        [JsonProperty("items4")]
        public GNPC Items4 { get; init; } = null!;

        [JsonProperty("items40")]
        public GNPC Items40 { get; init; } = null!;

        [JsonProperty("items41")]
        public GNPC Items41 { get; init; } = null!;

        [JsonProperty("items42")]
        public GNPC Items42 { get; init; } = null!;

        [JsonProperty("items43")]
        public GNPC Items43 { get; init; } = null!;

        [JsonProperty("items44")]
        public GNPC Items44 { get; init; } = null!;

        [JsonProperty("items45")]
        public GNPC Items45 { get; init; } = null!;

        [JsonProperty("items46")]
        public GNPC Items46 { get; init; } = null!;

        [JsonProperty("items47")]
        public GNPC Items47 { get; init; } = null!;

        [JsonProperty("items5")]
        public GNPC Items5 { get; init; } = null!;

        [JsonProperty("items6")]
        public GNPC Items6 { get; init; } = null!;

        [JsonProperty("items7")]
        public GNPC Items7 { get; init; } = null!;

        [JsonProperty("items8")]
        public GNPC Items8 { get; init; } = null!;

        [JsonProperty("items9")]
        public GNPC Items9 { get; init; } = null!;

        [JsonProperty("jailer")]
        public GNPC Jailer { get; init; } = null!;

        [JsonProperty("leathermerchant")]
        public GNPC Leathermerchant { get; init; } = null!;

        [JsonProperty("lichteaser")]
        public GNPC Lichteaser { get; init; } = null!;

        [JsonProperty("locksmith")]
        public GNPC Locksmith { get; init; } = null!;

        [JsonProperty("lostandfound")]
        public GNPC Lostandfound { get; init; } = null!;

        [JsonProperty("lotterylady")]
        public GNPC Lotterylady { get; init; } = null!;

        [JsonProperty("mcollector")]
        public GNPC Mcollector { get; init; } = null!;

        [JsonProperty("mistletoe")]
        public GNPC Mistletoe { get; init; } = null!;

        [JsonProperty("monsterhunter")]
        public GNPC Monsterhunter { get; init; } = null!;

        [JsonProperty("newupgrade")]
        public GNPC Newupgrade { get; init; } = null!;

        [JsonProperty("newyear_tree")]
        public GNPC NewyearTree { get; init; } = null!;

        [JsonProperty("ornaments")]
        public GNPC Ornaments { get; init; } = null!;

        [JsonProperty("pete")]
        public GNPC Pete { get; init; } = null!;

        [JsonProperty("pots")]
        public GNPC Pots { get; init; } = null!;

        [JsonProperty("premium")]
        public GNPC Premium { get; init; } = null!;

        [JsonProperty("princess")]
        public GNPC Princess { get; init; } = null!;

        [JsonProperty("pvp")]
        public GNPC Pvp { get; init; } = null!;

        [JsonProperty("pvpblocker")]
        public GNPC Pvpblocker { get; init; } = null!;

        [JsonProperty("pvptokens")]
        public GNPC Pvptokens { get; init; } = null!;

        [JsonProperty("pwincess")]
        public GNPC Pwincess { get; init; } = null!;

        [JsonProperty("rewards")]
        public GNPC Rewards { get; init; } = null!;

        [JsonProperty("santa")]
        public GNPC Santa { get; init; } = null!;

        [JsonProperty("scrolls")]
        public GNPC Scrolls { get; init; } = null!;

        [JsonProperty("secondhands")]
        public GNPC Secondhands { get; init; } = null!;

        [JsonProperty("shellsguy")]
        public GNPC Shellsguy { get; init; } = null!;

        [JsonProperty("ship")]
        public GNPC Ship { get; init; } = null!;

        [JsonProperty("shrine")]
        public GNPC Shrine { get; init; } = null!;

        [JsonProperty("standmerchant")]
        public GNPC Standmerchant { get; init; } = null!;

        [JsonProperty("tavern")]
        public GNPC Tavern { get; init; } = null!;

        [JsonProperty("tbartender")]
        public GNPC Tbartender { get; init; } = null!;

        [JsonProperty("thief")]
        public GNPC Thief { get; init; } = null!;

        [JsonProperty("transporter")]
        public GNPC Transporter { get; init; } = null!;

        [JsonProperty("wbartender")]
        public GNPC Wbartender { get; init; } = null!;

        [JsonProperty("weapons")]
        public GNPC Weapons { get; init; } = null!;

        [JsonProperty("witch")]
        public GNPC Witch { get; init; } = null!;

        [JsonProperty("wizardrepeater")]
        public GNPC Wizardrepeater { get; init; } = null!;

        [JsonProperty("wnpc")]
        public GNPC Wnpc { get; init; } = null!;
    }
}