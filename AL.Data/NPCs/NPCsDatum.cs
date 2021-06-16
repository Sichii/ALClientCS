using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.NPCs
{
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<NPC>))]
    public class NPCsDatum : DatumBase<NPC>
    {
        public NPC Antip2W { get; set; }
        public NPC Appearance { get; set; }
        public NPC Armors { get; set; }
        public NPC Basics { get; set; }
        public NPC Bean { get; set; }
        public NPC Beans { get; set; }
        public NPC Bouncer { get; set; }
        public NPC Citizen0 { get; set; }
        public NPC Citizen1 { get; set; }
        public NPC Citizen10 { get; set; }
        public NPC Citizen11 { get; set; }
        public NPC Citizen12 { get; set; }
        public NPC Citizen13 { get; set; }
        public NPC Citizen14 { get; set; }
        public NPC Citizen15 { get; set; }
        public NPC Citizen2 { get; set; }
        public NPC Citizen3 { get; set; }
        public NPC Citizen4 { get; set; }
        public NPC Citizen5 { get; set; }
        public NPC Citizen6 { get; set; }
        public NPC Citizen7 { get; set; }
        public NPC Citizen8 { get; set; }
        public NPC Citizen9 { get; set; }
        public NPC Compound { get; set; }
        public NPC Craftsman { get; set; }
        public NPC Exchange { get; set; }
        public NPC FancyPots { get; set; }
        public NPC FirstC { get; set; }
        public NPC FisherMan { get; set; }
        public NPC FunTokens { get; set; }
        public NPC GeMmerchant { get; set; }
        public NPC GoldNPC { get; set; }
        public NPC Guard { get; set; }
        public NPC Holo { get; set; }
        public NPC Holo0 { get; set; }
        public NPC Holo1 { get; set; }
        public NPC Holo2 { get; set; }
        public NPC Holo3 { get; set; }
        public NPC Holo4 { get; set; }
        public NPC Holo5 { get; set; }
        public NPC Items0 { get; set; }
        public NPC Items1 { get; set; }
        public NPC Items10 { get; set; }
        public NPC Items11 { get; set; }
        public NPC Items12 { get; set; }
        public NPC Items13 { get; set; }
        public NPC Items14 { get; set; }
        public NPC Items15 { get; set; }
        public NPC Items16 { get; set; }
        public NPC Items17 { get; set; }
        public NPC Items18 { get; set; }
        public NPC Items19 { get; set; }
        public NPC Items2 { get; set; }
        public NPC Items20 { get; set; }
        public NPC Items21 { get; set; }
        public NPC Items22 { get; set; }
        public NPC Items23 { get; set; }
        public NPC Items24 { get; set; }
        public NPC Items25 { get; set; }
        public NPC Items26 { get; set; }
        public NPC Items27 { get; set; }
        public NPC Items28 { get; set; }
        public NPC Items29 { get; set; }
        public NPC Items3 { get; set; }
        public NPC Items30 { get; set; }
        public NPC Items31 { get; set; }
        public NPC Items32 { get; set; }
        public NPC Items33 { get; set; }
        public NPC Items34 { get; set; }
        public NPC Items35 { get; set; }
        public NPC Items36 { get; set; }
        public NPC Items37 { get; set; }
        public NPC Items38 { get; set; }
        public NPC Items39 { get; set; }
        public NPC Items4 { get; set; }
        public NPC Items40 { get; set; }
        public NPC Items41 { get; set; }
        public NPC Items42 { get; set; }
        public NPC Items43 { get; set; }
        public NPC Items44 { get; set; }
        public NPC Items45 { get; set; }
        public NPC Items46 { get; set; }
        public NPC Items47 { get; set; }
        public NPC Items5 { get; set; }
        public NPC Items6 { get; set; }
        public NPC Items7 { get; set; }
        public NPC Items8 { get; set; }
        public NPC Items9 { get; set; }
        public NPC Jailer { get; set; }
        public NPC LeatherMerchant { get; set; }
        public NPC LichTeaser { get; set; }
        public NPC LockSmith { get; set; }
        public NPC LostAndFound { get; set; }
        public NPC LotteryLady { get; set; }
        public NPC MCollector { get; set; }
        public NPC Mistletoe { get; set; }
        public NPC MonsterHunter { get; set; }
        public NPC NewUpgrade { get; set; }

        [JsonProperty("newyear_tree")]
        public NPC NewYearTree { get; set; }

        public NPC Ornaments { get; set; }
        public NPC Pete { get; set; }
        public NPC Pots { get; set; }
        public NPC Premium { get; set; }
        public NPC Princess { get; set; }
        public NPC Pvp { get; set; }
        public NPC PvPBlocker { get; set; }
        public NPC PvPTokens { get; set; }
        public NPC Pwincess { get; set; }
        public NPC Rewards { get; set; }
        public NPC Santa { get; set; }
        public NPC Scrolls { get; set; }
        public NPC SecondHands { get; set; }
        public NPC ShellsGuy { get; set; }
        public NPC Ship { get; set; }
        public NPC Shrine { get; set; }
        public NPC StandMerchant { get; set; }
        public NPC Tavern { get; set; }

        // ReSharper disable once InconsistentNaming
        public NPC TBartender { get; set; }
        public NPC Thief { get; set; }
        public NPC Transporter { get; set; }
        public NPC WBartender { get; set; }
        public NPC Weapons { get; set; }
        public NPC Witch { get; set; }
        public NPC WizardRepeater { get; set; }
        public NPC WNPC { get; set; }
    }
}