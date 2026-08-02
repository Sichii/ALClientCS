namespace AL.Core.Definitions;

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
    ///     How far an entity may be before the client stops believing in it. The server sends nothing when an entity
    ///     leaves your view - it just stops mentioning it - so dropping one is the client's job and its own reckoning.
    ///     The server refuses a targeted skill past 1000 regardless, which is the ceiling this sits under.
    /// </summary>
    public const float MAX_VISION = 800;

    /// <summary>
    ///     Center to center
    /// </summary>
    public const float NPC_RANGE = 400f * 0.975f;

    /// <summary>
    ///     Unknown
    /// </summary>
    public const float TRANSPORTER_RANGE = 150f * 0.975f;

    /// <summary>
    ///     The edge-to-edge range for trading
    /// </summary>
    public const float TRADE_RANGE = 300;
}