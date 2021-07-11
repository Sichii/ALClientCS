namespace AL.Data.Achievements
{
    public record Achievement
    {
        /// <summary>
        ///     Amount of kills needed to earn this achievement.
        /// </summary>
        public int Count { get; init; }
        /// <summary>
        ///     How to earn the achievement.
        /// </summary>
        public string? Explanation { get; init; }
        /// <summary>
        ///     Sometimes different than the title.
        /// </summary>
        public string Name { get; init; } = null!;
        /// <summary>
        ///     The number of shells rewarded for earning this achievement.
        /// </summary>
        public int Shells { get; init; }
        /// <summary>
        ///     Sometimes different than the name.
        /// </summary>
        public string? Title { get; init; }
    }
}