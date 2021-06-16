using Newtonsoft.Json;

namespace AL.Data.Achievements
{
    public class AchievementsDatum : DatumBase<Achievement>
    {
        [JsonProperty("1000boss")]
        public Achievement _1000boss { get; init; }

        [JsonProperty("100boss")]
        public Achievement _100boss { get; init; }

        public Achievement Discoverlair { get; init; }
        public Achievement Festive { get; init; }
        public Achievement Firehazard { get; init; }
        public Achievement Gooped { get; init; }
        public Achievement Lucky { get; init; }
        public Achievement Monsterhunter { get; init; }
        public Achievement Reach40 { get; init; }
        public Achievement Reach50 { get; init; }
        public Achievement Reach60 { get; init; }
        public Achievement Reach70 { get; init; }
        public Achievement Reach80 { get; init; }
        public Achievement Reach90 { get; init; }
        public Achievement Stomped { get; init; }
        public Achievement Upgrade10 { get; init; }
    }
}