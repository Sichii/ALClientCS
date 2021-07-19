using AL.Core.Helpers;

namespace AL.Client.Model
{
    /// <summary>
    ///     Represents the instant in time in which the server confirmed that a skill was used.
    /// </summary>
    /// <param name="CooldownMS">The cooldown of the skill used..</param>
    public record CooldownInfo(float CooldownMS)
    {
        private readonly long Delta = DeltaTime.Value;

        /// <summary>
        ///     Whether or not the skill can be used.
        /// </summary>
        /// <param name="offsetMS">The current ping offset to use when deciding if the skill is useable.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the skill can be used, otherwise <c>false</c>.
        /// </returns>
        public bool CanUse(int offsetMS) => DeltaTime.Value - Delta + offsetMS > CooldownMS;
    }
}