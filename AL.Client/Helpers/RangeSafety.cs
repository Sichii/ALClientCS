#region
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     How much of a skill's range is actually spent before a range check answers. The shave exists because the positions
///     the check reads and the positions the server resolves the cast against are not the same positions: the client
///     reckons its own walk between server frames and ping-compensates every entity it can see, so a cast fired at the
///     very edge lands after both ends have moved on.
/// </summary>
/// <remarks>
///     One number for every case shaved the same 5% off a pair standing still, where the two readings agree exactly and
///     the whole reach is real - which for melee is several units of stand-off bought for nothing. So the margin is sized
///     to how far the two can drift while the cast is in flight, and that is a question about which way each of them is
///     walking.
/// </remarks>
public static class RangeSafety
{
    /// <summary>
    ///     Both ends walking away from each other, which is the fastest the gap can open, so the full shave. The tightest
    ///     margin this class can return, which is the number every stopping distance is sized with.
    /// </summary>
    public const float MOVING_APART = 0.95f;

    /// <summary>
    ///     Anything else: one end standing, one closing, or a leg going nowhere. The gap still moves, but no faster than one
    ///     of the two can walk.
    /// </summary>
    public const float MIXED = 0.975f;

    /// <summary>
    ///     Neither end moving. There is nothing to compensate for - the client is not reckoning a walk it has not been told
    ///     about, and both ends read where the server has them - so the whole range is real.
    /// </summary>
    public const float STANDING = 1f;

    /// <summary>
    ///     The margin a range check between these two should spend.
    /// </summary>
    public static float MarginFor(MovementBlock source, MovementBlock target)
    {
        if (source.MovingAwayFrom(target) && target.MovingAwayFrom(source))
            return MOVING_APART;

        if (!source.Moving && !target.Moving)
            return STANDING;

        return MIXED;
    }
}