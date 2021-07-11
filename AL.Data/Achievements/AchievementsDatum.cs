using Newtonsoft.Json;

namespace AL.Data.Achievements
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class AchievementsDatum : DatumBase<Achievement>
    {
        /// <summary>
        ///     Defeat 1,000 Bosses.
        /// </summary>
        [JsonProperty("1000boss")]
        public Achievement _1000boss { get; init; } = null!;

        /// <summary>
        ///     Defeat 100 Bosses.
        /// </summary>
        [JsonProperty("100boss")]
        public Achievement _100boss { get; init; } = null!;
        /// <summary>
        ///     Find the lair of the Spider Queen.
        /// </summary>
        public Achievement Discoverlair { get; init; } = null!;
        /// <summary>
        ///     Deal 400K damage to Grinch.
        /// </summary>
        public Achievement Festive { get; init; } = null!;
        /// <summary>
        ///     Last hit 20,000 monsters consecutively with only burn damage using same weapon!
        /// </summary>
        public Achievement Firehazard { get; init; } = null!;
        /// <summary>
        ///     Receive 60M damage from Goo's.
        /// </summary>
        public Achievement Gooped { get; init; } = null!;
        /// <summary>
        ///     Succeed with the exact % on an upgrade or compound.
        /// </summary>
        public Achievement Lucky { get; init; } = null!;
        /// <summary>
        ///     Kill 1,000,000 Monsters.
        /// </summary>
        public Achievement Monsterhunter { get; init; } = null!;
        /// <summary>
        ///     Become Level 40.
        /// </summary>
        public Achievement Reach40 { get; init; } = null!;
        /// <summary>
        ///     Become Level 50.
        /// </summary>
        public Achievement Reach50 { get; init; } = null!;
        /// <summary>
        ///     Become Level 60.
        /// </summary>
        public Achievement Reach60 { get; init; } = null!;
        /// <summary>
        ///     Become Level 70.
        /// </summary>
        public Achievement Reach70 { get; init; } = null!;
        /// <summary>
        ///     Become Level 80.
        /// </summary>
        public Achievement Reach80 { get; init; } = null!;
        /// <summary>
        ///     Become Level 90
        /// </summary>
        public Achievement Reach90 { get; init; } = null!;
        /// <summary>
        ///     Get hit 1,200 times randomly by Stompy, without getting hit by any other monster!
        /// </summary>
        public Achievement Stomped { get; init; } = null!;
        /// <summary>
        ///     Upgrade an item to +X.
        /// </summary>
        public Achievement Upgrade10 { get; init; } = null!;
    }
}