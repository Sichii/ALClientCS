namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when an achievement gains some progress.
    /// </summary>
    public record AchievementProgressData
    {
        /// <summary>
        ///     The amount of kills you currently have for this achievement.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     The name of the achievement.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        ///     The amount of kills needed for this achievement.
        /// </summary>
        public int Needed { get; set; }
    }
}