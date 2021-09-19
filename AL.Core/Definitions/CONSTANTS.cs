namespace AL.Core.Definitions
{
    /// <summary>
    ///     Provides assembly level compile time values
    /// </summary>
    public static class CONSTANTS
    {
        /// <summary>
        ///     Center to center?
        /// </summary>
        public const float DOOR_RANGE = 40f * 0.975f;
        /// <summary>
        ///     A default equality descriminator for floating point arithmetic specific to this library's use case.
        /// </summary>
        public const float EPSILON = 0.0001f;

        /// <summary>
        ///     Center to center
        /// </summary>
        public const float NPC_RANGE = 400f * 0.975f;

        /// <summary>
        ///     Unknown
        /// </summary>
        public const float TRANSPORTER_RANGE = 150f * 0.975f;
    }
}