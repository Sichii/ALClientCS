namespace AL.Data.Monsters
{
    /// <summary>
    ///     Represents a condition that a monster has when it spawns. <br />
    ///     No information aside from duration is generally given.
    /// </summary>
    public record InitialCondition
    {
        /// <summary>
        ///     The duration in milliseconds.
        /// </summary>
        public float DurationMS { get; init; }
    }
}