using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.NPCs
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GNPC>))]
    public class NPCsDatum : DatumBase<GNPC>
    {
        public GNPC Antip2W { get; set; } = null!;
        public GNPC Appearance { get; set; } = null!;
        public GNPC Armors { get; set; } = null!;
        public GNPC Basics { get; set; } = null!;
        public GNPC Bean { get; set; } = null!;
        public GNPC Beans { get; set; } = null!;
        public GNPC Bouncer { get; set; } = null!;
        public GNPC Citizen0 { get; set; } = null!;
        public GNPC Citizen1 { get; set; } = null!;
        public GNPC Citizen10 { get; set; } = null!;
        public GNPC Citizen11 { get; set; } = null!;
        public GNPC Citizen12 { get; set; } = null!;
        public GNPC Citizen13 { get; set; } = null!;
        public GNPC Citizen14 { get; set; } = null!;
        public GNPC Citizen15 { get; set; } = null!;
        public GNPC Citizen2 { get; set; } = null!;
        public GNPC Citizen3 { get; set; } = null!;
        public GNPC Citizen4 { get; set; } = null!;
        public GNPC Citizen5 { get; set; } = null!;
        public GNPC Citizen6 { get; set; } = null!;
        public GNPC Citizen7 { get; set; } = null!;
        public GNPC Citizen8 { get; set; } = null!;
        public GNPC Citizen9 { get; set; } = null!;
        public GNPC Compound { get; set; } = null!;
        public GNPC Craftsman { get; set; } = null!;
        public GNPC Exchange { get; set; } = null!;
        public GNPC FancyPots { get; set; } = null!;
        public GNPC FirstC { get; set; } = null!;
        public GNPC FisherMan { get; set; } = null!;
        public GNPC FunTokens { get; set; } = null!;
        public GNPC GeMmerchant { get; set; } = null!;
        public GNPC GoldNPC { get; set; } = null!;
        public GNPC Guard { get; set; } = null!;
        public GNPC Holo { get; set; } = null!;
        public GNPC Holo0 { get; set; } = null!;
        public GNPC Holo1 { get; set; } = null!;
        public GNPC Holo2 { get; set; } = null!;
        public GNPC Holo3 { get; set; } = null!;
        public GNPC Holo4 { get; set; } = null!;
        public GNPC Holo5 { get; set; } = null!;
        public GNPC Items0 { get; set; } = null!;
        public GNPC Items1 { get; set; } = null!;
        public GNPC Items10 { get; set; } = null!;
        public GNPC Items11 { get; set; } = null!;
        public GNPC Items12 { get; set; } = null!;
        public GNPC Items13 { get; set; } = null!;
        public GNPC Items14 { get; set; } = null!;
        public GNPC Items15 { get; set; } = null!;
        public GNPC Items16 { get; set; } = null!;
        public GNPC Items17 { get; set; } = null!;
        public GNPC Items18 { get; set; } = null!;
        public GNPC Items19 { get; set; } = null!;
        public GNPC Items2 { get; set; } = null!;
        public GNPC Items20 { get; set; } = null!;
        public GNPC Items21 { get; set; } = null!;
        public GNPC Items22 { get; set; } = null!;
        public GNPC Items23 { get; set; } = null!;
        public GNPC Items24 { get; set; } = null!;
        public GNPC Items25 { get; set; } = null!;
        public GNPC Items26 { get; set; } = null!;
        public GNPC Items27 { get; set; } = null!;
        public GNPC Items28 { get; set; } = null!;
        public GNPC Items29 { get; set; } = null!;
        public GNPC Items3 { get; set; } = null!;
        public GNPC Items30 { get; set; } = null!;
        public GNPC Items31 { get; set; } = null!;
        public GNPC Items32 { get; set; } = null!;
        public GNPC Items33 { get; set; } = null!;
        public GNPC Items34 { get; set; } = null!;
        public GNPC Items35 { get; set; } = null!;
        public GNPC Items36 { get; set; } = null!;
        public GNPC Items37 { get; set; } = null!;
        public GNPC Items38 { get; set; } = null!;
        public GNPC Items39 { get; set; } = null!;
        public GNPC Items4 { get; set; } = null!;
        public GNPC Items40 { get; set; } = null!;
        public GNPC Items41 { get; set; } = null!;
        public GNPC Items42 { get; set; } = null!;
        public GNPC Items43 { get; set; } = null!;
        public GNPC Items44 { get; set; } = null!;
        public GNPC Items45 { get; set; } = null!;
        public GNPC Items46 { get; set; } = null!;
        public GNPC Items47 { get; set; } = null!;
        public GNPC Items5 { get; set; } = null!;
        public GNPC Items6 { get; set; } = null!;
        public GNPC Items7 { get; set; } = null!;
        public GNPC Items8 { get; set; } = null!;
        public GNPC Items9 { get; set; } = null!;
        public GNPC Jailer { get; set; } = null!;
        public GNPC LeatherMerchant { get; set; } = null!;
        public GNPC LichTeaser { get; set; } = null!;
        public GNPC LockSmith { get; set; } = null!;
        public GNPC LostAndFound { get; set; } = null!;
        public GNPC LotteryLady { get; set; } = null!;
        public GNPC MCollector { get; set; } = null!;
        public GNPC Mistletoe { get; set; } = null!;
        public GNPC MonsterHunter { get; set; } = null!;
        public GNPC NewUpgrade { get; set; } = null!;

        [JsonProperty("newyear_tree")]
        public GNPC NewYearTree { get; set; } = null!;

        public GNPC Ornaments { get; set; } = null!;
        public GNPC Pete { get; set; } = null!;
        public GNPC Pots { get; set; } = null!;
        public GNPC Premium { get; set; } = null!;
        public GNPC Princess { get; set; } = null!;
        public GNPC Pvp { get; set; } = null!;
        public GNPC PvPBlocker { get; set; } = null!;
        public GNPC PvPTokens { get; set; } = null!;
        public GNPC Pwincess { get; set; } = null!;
        public GNPC Rewards { get; set; } = null!;
        public GNPC Santa { get; set; } = null!;
        public GNPC Scrolls { get; set; } = null!;
        public GNPC SecondHands { get; set; } = null!;
        public GNPC ShellsGuy { get; set; } = null!;
        public GNPC Ship { get; set; } = null!;
        public GNPC Shrine { get; set; } = null!;
        public GNPC StandMerchant { get; set; } = null!;
        public GNPC Tavern { get; set; } = null!;

        // ReSharper disable once InconsistentNaming
        public GNPC TBartender { get; set; } = null!;
        public GNPC Thief { get; set; } = null!;
        public GNPC Transporter { get; set; } = null!;
        public GNPC WBartender { get; set; } = null!;
        public GNPC Weapons { get; set; } = null!;
        public GNPC Witch { get; set; } = null!;
        public GNPC WizardRepeater { get; set; } = null!;
        public GNPC WNPC { get; set; } = null!;
    }
}