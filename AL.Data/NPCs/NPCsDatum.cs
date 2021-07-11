using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.NPCs
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<NPC>))]
    public class NPCsDatum : DatumBase<NPC>
    {
        public NPC Antip2W { get; set; } = null!;
        public NPC Appearance { get; set; } = null!;
        public NPC Armors { get; set; } = null!;
        public NPC Basics { get; set; } = null!;
        public NPC Bean { get; set; } = null!;
        public NPC Beans { get; set; } = null!;
        public NPC Bouncer { get; set; } = null!;
        public NPC Citizen0 { get; set; } = null!;
        public NPC Citizen1 { get; set; } = null!;
        public NPC Citizen10 { get; set; } = null!;
        public NPC Citizen11 { get; set; } = null!;
        public NPC Citizen12 { get; set; } = null!;
        public NPC Citizen13 { get; set; } = null!;
        public NPC Citizen14 { get; set; } = null!;
        public NPC Citizen15 { get; set; } = null!;
        public NPC Citizen2 { get; set; } = null!;
        public NPC Citizen3 { get; set; } = null!;
        public NPC Citizen4 { get; set; } = null!;
        public NPC Citizen5 { get; set; } = null!;
        public NPC Citizen6 { get; set; } = null!;
        public NPC Citizen7 { get; set; } = null!;
        public NPC Citizen8 { get; set; } = null!;
        public NPC Citizen9 { get; set; } = null!;
        public NPC Compound { get; set; } = null!;
        public NPC Craftsman { get; set; } = null!;
        public NPC Exchange { get; set; } = null!;
        public NPC FancyPots { get; set; } = null!;
        public NPC FirstC { get; set; } = null!;
        public NPC FisherMan { get; set; } = null!;
        public NPC FunTokens { get; set; } = null!;
        public NPC GeMmerchant { get; set; } = null!;
        public NPC GoldNPC { get; set; } = null!;
        public NPC Guard { get; set; } = null!;
        public NPC Holo { get; set; } = null!;
        public NPC Holo0 { get; set; } = null!;
        public NPC Holo1 { get; set; } = null!;
        public NPC Holo2 { get; set; } = null!;
        public NPC Holo3 { get; set; } = null!;
        public NPC Holo4 { get; set; } = null!;
        public NPC Holo5 { get; set; } = null!;
        public NPC Items0 { get; set; } = null!;
        public NPC Items1 { get; set; } = null!;
        public NPC Items10 { get; set; } = null!;
        public NPC Items11 { get; set; } = null!;
        public NPC Items12 { get; set; } = null!;
        public NPC Items13 { get; set; } = null!;
        public NPC Items14 { get; set; } = null!;
        public NPC Items15 { get; set; } = null!;
        public NPC Items16 { get; set; } = null!;
        public NPC Items17 { get; set; } = null!;
        public NPC Items18 { get; set; } = null!;
        public NPC Items19 { get; set; } = null!;
        public NPC Items2 { get; set; } = null!;
        public NPC Items20 { get; set; } = null!;
        public NPC Items21 { get; set; } = null!;
        public NPC Items22 { get; set; } = null!;
        public NPC Items23 { get; set; } = null!;
        public NPC Items24 { get; set; } = null!;
        public NPC Items25 { get; set; } = null!;
        public NPC Items26 { get; set; } = null!;
        public NPC Items27 { get; set; } = null!;
        public NPC Items28 { get; set; } = null!;
        public NPC Items29 { get; set; } = null!;
        public NPC Items3 { get; set; } = null!;
        public NPC Items30 { get; set; } = null!;
        public NPC Items31 { get; set; } = null!;
        public NPC Items32 { get; set; } = null!;
        public NPC Items33 { get; set; } = null!;
        public NPC Items34 { get; set; } = null!;
        public NPC Items35 { get; set; } = null!;
        public NPC Items36 { get; set; } = null!;
        public NPC Items37 { get; set; } = null!;
        public NPC Items38 { get; set; } = null!;
        public NPC Items39 { get; set; } = null!;
        public NPC Items4 { get; set; } = null!;
        public NPC Items40 { get; set; } = null!;
        public NPC Items41 { get; set; } = null!;
        public NPC Items42 { get; set; } = null!;
        public NPC Items43 { get; set; } = null!;
        public NPC Items44 { get; set; } = null!;
        public NPC Items45 { get; set; } = null!;
        public NPC Items46 { get; set; } = null!;
        public NPC Items47 { get; set; } = null!;
        public NPC Items5 { get; set; } = null!;
        public NPC Items6 { get; set; } = null!;
        public NPC Items7 { get; set; } = null!;
        public NPC Items8 { get; set; } = null!;
        public NPC Items9 { get; set; } = null!;
        public NPC Jailer { get; set; } = null!;
        public NPC LeatherMerchant { get; set; } = null!;
        public NPC LichTeaser { get; set; } = null!;
        public NPC LockSmith { get; set; } = null!;
        public NPC LostAndFound { get; set; } = null!;
        public NPC LotteryLady { get; set; } = null!;
        public NPC MCollector { get; set; } = null!;
        public NPC Mistletoe { get; set; } = null!;
        public NPC MonsterHunter { get; set; } = null!;
        public NPC NewUpgrade { get; set; } = null!;

        [JsonProperty("newyear_tree")]
        public NPC NewYearTree { get; set; } = null!;

        public NPC Ornaments { get; set; } = null!;
        public NPC Pete { get; set; } = null!;
        public NPC Pots { get; set; } = null!;
        public NPC Premium { get; set; } = null!;
        public NPC Princess { get; set; } = null!;
        public NPC Pvp { get; set; } = null!;
        public NPC PvPBlocker { get; set; } = null!;
        public NPC PvPTokens { get; set; } = null!;
        public NPC Pwincess { get; set; } = null!;
        public NPC Rewards { get; set; } = null!;
        public NPC Santa { get; set; } = null!;
        public NPC Scrolls { get; set; } = null!;
        public NPC SecondHands { get; set; } = null!;
        public NPC ShellsGuy { get; set; } = null!;
        public NPC Ship { get; set; } = null!;
        public NPC Shrine { get; set; } = null!;
        public NPC StandMerchant { get; set; } = null!;
        public NPC Tavern { get; set; } = null!;

        // ReSharper disable once InconsistentNaming
        public NPC TBartender { get; set; } = null!;
        public NPC Thief { get; set; } = null!;
        public NPC Transporter { get; set; } = null!;
        public NPC WBartender { get; set; } = null!;
        public NPC Weapons { get; set; } = null!;
        public NPC Witch { get; set; } = null!;
        public NPC WizardRepeater { get; set; } = null!;
        public NPC WNPC { get; set; } = null!;
    }
}