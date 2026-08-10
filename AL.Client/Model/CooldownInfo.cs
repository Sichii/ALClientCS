#region
using AL.Core.Interfaces;
using AL.SocketClient.Interfaces;
using Chaos.Time.Abstractions;
#endregion

namespace AL.Client.Model;

/// <summary>
///     Represents the instant in time in which the server confirmed that a skill was used.
/// </summary>
public sealed class CooldownInfo : IPingCompensated, IDeltaUpdatable
{
    /// <summary>
    ///     How far short of the round trip <see cref="CompensateOnce" /> stops, so the next use aims just past the
    ///     server's expiry rather than exactly at it.
    /// </summary>
    /// <remarks>
    ///     Load-bearing since the offset became a 5th percentile of the ping window rather than its minimum. The
    ///     percentile sits a measured 2.5ms above that minimum on a typical window and under 7ms on 95% of them, and
    ///     this guard is what absorbs that. Shrink it below the gap and roughly one emit in twenty reaches the server
    ///     before its cooldown expires, which the server refuses outright.
    /// </remarks>
    private const float JITTER_GUARD_MS = 15f;

    /// <summary>
    ///     The cooldown of the skill.
    /// </summary>
    public float CooldownMs { get; init; }

    /// <summary>
    ///     Gets the amount of milliseconds that have elapsed since a skill was used.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    public bool IsCompensated { get; private set; }

    //TODO: maybe make this the skill name, but there's really no point
    string IMutable.Id => string.Empty;

    /// <summary>
    ///     Gets the remaining cooldown in milliseconds.
    /// </summary>
    public float RemainingMS => CooldownMs - (float)Elapsed.TotalMilliseconds;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CooldownInfo" /> class.
    /// </summary>
    /// <param name="cooldownMs">
    ///     The cooldown of the skill.
    /// </param>
    public CooldownInfo(float cooldownMs) => CooldownMs = cooldownMs;

    public void CompensateOnce(TimeSpan offset)
    {
        if (IsCompensated)
            throw new InvalidOperationException("Object already compensated.");

        IsCompensated = true;

        //compensating the whole round trip aims the next use at the exact instant the server's timer expires, and the
        //server keeps no grace - it refuses outright while mssince(last) is under the cooldown - so any leg quicker
        //than the offset lands early and is rejected. The offset is a low percentile rather than the window's
        //minimum, so that is roughly one leg in twenty, early by the few ms the percentile sits above that minimum.
        //The guard covers it and is well inside the poll granularity of anything waiting on this
        Elapsed += offset - TimeSpan.FromMilliseconds(JITTER_GUARD_MS);
    }

    /// <inheritdoc />
    public void Update(TimeSpan delta) => Elapsed += delta;

    /// <summary>
    ///     Whether or not the skill can be used.
    /// </summary>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if the skill can be used, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public bool CanUse() => Elapsed.TotalMilliseconds > CooldownMs;
}