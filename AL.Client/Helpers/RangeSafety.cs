#region
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the fraction of a skill's range a range check spends. The client reckons its own walk between server
///     frames and ping-compensates every entity it sees, so the margin is sized to how far the two ends can drift during
///     the cast.
/// </summary>
public static class RangeSafety
{
    /// <summary>
    ///     The margin when both ends walk away from each other, the fastest the gap can open. The tightest margin this class
    ///     returns, which every stopping distance is sized with.
    /// </summary>
    public const float MOVING_APART = 0.95f;

    /// <summary>
    ///     The margin when one end stands or closes while the other moves, or a leg goes nowhere.
    /// </summary>
    public const float MIXED = 0.975f;

    /// <summary>
    ///     The margin when neither end moves, where both ends read where the server has them and the whole range is real.
    /// </summary>
    public const float STANDING = 1f;

    /// <summary>
    ///     Calculates the margin a range check between these two should spend.
    /// </summary>
    /// <param name="source">The movement of the end casting.</param>
    /// <param name="target">The movement of the end being cast on.</param>
    /// <returns>The fraction of the range to spend.</returns>
    public static float CalculateMargin(MovementBlock source, MovementBlock target)
    {
        if (source.MovingAwayFrom(target) && target.MovingAwayFrom(source))
            return MOVING_APART;

        if (!source.Moving && !target.Moving)
            return STANDING;

        return MIXED;
    }
}