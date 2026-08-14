namespace AL.Core.Definitions;

/// <summary>
///     Provides assembly level compile time values
/// </summary>
public static class CONSTANTS
{
    /// <summary>
    ///     How much of a server limit this library will claim. Every range below that answers "may I act from here"
    ///     is the server's own number through this.
    /// </summary>
    /// <remarks>
    ///     What it buys is the difference between where the client reckons the character is and where the server has
    ///     it when the emit lands a moment later. A walk is where that bites hardest: the pathfinder does not merely
    ///     stay inside the region an exit accepts, it stops the walk exactly on that region's edge, because the
    ///     shortcut aims at the nearest point of a reach circle. So the shave is the whole of the margin, not a
    ///     tidy-up on top of one.
    ///     <br />
    ///     The 2.5% this replaced left 2.8 units on a door and was not enough: every door and transporter refused
    ///     with <c>transport_cant_reach</c>, from stop points the server measured three to four units past its
    ///     limit. Doubling it leaves 5.6 units on a door and 8 on a transporter, which is the smaller side of what
    ///     that measurement supports - an exit that still refuses is this number to raise.
    /// </remarks>
    public const float RANGE_SHAVE = 0.95f;

    /// <summary>
    ///     How close a door lets you through. Not a centre-to-centre radius - the server measures boxes, and an
    ///     exit's <c>ReachableFrom</c> is the region this builds.
    /// </summary>
    public const float DOOR_RANGE = 112f * RANGE_SHAVE;

    /// <summary>
    ///     The width of a character's sprite box, which the server folds into a door's range check. This is not the
    ///     collision base the nav mesh pads walls with - the server keeps those two apart, and measures a door with
    ///     this one.
    /// </summary>
    public const float CHARACTER_BOX_WIDTH = 26f;

    /// <summary>
    ///     The height of a character's sprite box. Applied on one side only, because the box hangs upward from the
    ///     character's position, as a door's box does from the spawn it sits on.
    /// </summary>
    public const float CHARACTER_BOX_HEIGHT = 36f;

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
    public const float NPC_RANGE = 400f * RANGE_SHAVE;

    /// <summary>
    ///     Center to center, and plainly euclidean - the transporter is the one exit the server measures with
    ///     <c>simple_distance</c> rather than the box test it uses on doors.
    /// </summary>
    public const float TRANSPORTER_RANGE = 160f * RANGE_SHAVE;

    /// <summary>
    ///     The edge-to-edge range for trading. The base is well under the server's own <c>B.dist</c> of 400 already,
    ///     and goes through the shave for the reason every other range here does.
    /// </summary>
    public const float TRADE_RANGE = 300f * RANGE_SHAVE;
}