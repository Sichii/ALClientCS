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

    public void CompensateOnce(TimeSpan minimumOffset)
    {
        if (IsCompensated)
            throw new InvalidOperationException("Object already compensated.");

        IsCompensated = true;

        //compensating the whole round trip aims the next use at the exact instant the server's timer expires, and the
        //server keeps no grace - it refuses outright while mssince(last) is under the cooldown - so any leg quicker
        //than the minimum observed one lands early and is rejected. Held back by the guard, which is well inside the
        //poll granularity of anything waiting on this and so costs no real uptime
        Elapsed += minimumOffset - TimeSpan.FromMilliseconds(JITTER_GUARD_MS);
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