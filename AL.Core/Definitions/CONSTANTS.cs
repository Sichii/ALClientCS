namespace AL.Core.Definitions;

/// <summary>Provides assembly level compile time values.</summary>
public static class CONSTANTS
{
    /// <summary>
    ///     The fraction of a server range this library claims. Every range below that answers "may I act from here" is the
    ///     server's own number scaled by this.
    /// </summary>
    /// <remarks>
    ///     A walk stops exactly on an exit's edge, so this is the whole margin for where the server has the character when the
    ///     emit lands. An exit that still refuses with <c>transport_cant_reach</c> is this number to raise.
    /// </remarks>
    public const float RANGE_SHAVE = 0.95f;

    /// <summary>
    ///     The range at which a door lets you through. Not a centre-to-centre radius - the server measures boxes, and an
    ///     exit's <c>ReachBand</c> inflated by this is the region it accepts.
    /// </summary>
    public const float DOOR_RANGE = 112f * RANGE_SHAVE;

    /// <summary>
    ///     The range at which a stair on a generated floor lets you through: a centre-to-centre radius about the landing the
    ///     stair names, far tighter than <see cref="DOOR_RANGE" />.
    /// </summary>
    public const float STAIR_RANGE = 40f * RANGE_SHAVE;

    /// <summary>
    ///     The width of a character's sprite box, which the server folds into a door's range check. This is not the collision
    ///     base the nav mesh pads walls with - the server keeps those two apart, and measures a door with this one.
    /// </summary>
    public const float CHARACTER_BOX_WIDTH = 26f;

    /// <summary>
    ///     The height of a character's sprite box. Applied on one side only, because the box hangs upward from the character's
    ///     position, as a door's box does from the spawn it sits on.
    /// </summary>
    public const float CHARACTER_BOX_HEIGHT = 36f;

    /// <summary>
    ///     A default equality descriminator for floating point arithmetic specific to this library's use case.
    /// </summary>
    public const float EPSILON = 0.0001f;

    /// <summary>
    ///     The distance past which the client drops an entity. The server sends nothing when an entity leaves your view, and
    ///     refuses a targeted skill past 1000 regardless.
    /// </summary>
    public const float MAX_VISION = 800;

    /// <summary>Center to center</summary>
    public const float NPC_RANGE = 400f * RANGE_SHAVE;

    /// <summary>
    ///     Center to center, and plainly euclidean - the transporter is the one exit the server measures with
    ///     <c>simple_distance</c> rather than the box test it uses on doors.
    /// </summary>
    public const float TRANSPORTER_RANGE = 160f * RANGE_SHAVE;

    /// <summary>
    ///     The edge-to-edge range for trading, well under the server's own <c>B.dist</c> of 400.
    /// </summary>
    public const float TRADE_RANGE = 300f * RANGE_SHAVE;
}