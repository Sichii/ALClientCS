#region
using System.Diagnostics;
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
    ///     The clock reading when this cooldown was recorded.
    /// </summary>
    private readonly long StartedAt = Stopwatch.GetTimestamp();

    /// <summary>
    ///     The time <see cref="CompensateOnce" /> counts as already gone.
    /// </summary>
    private TimeSpan Offset;

    /// <summary>The cooldown of the skill.</summary>
    public float CooldownMs { get; init; }

    /// <summary>
    ///     The time that has elapsed since the skill was used, read off the clock.
    /// </summary>
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(StartedAt) + Offset;

    public bool IsCompensated { get; private set; }

    /// <summary>Gets the remaining cooldown in milliseconds.</summary>
    public float RemainingMS => CooldownMs - (float)Elapsed.TotalMilliseconds;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CooldownInfo" /> class.
    /// </summary>
    /// <param name="cooldownMs">The cooldown of the skill.</param>
    public CooldownInfo(float cooldownMs) => CooldownMs = cooldownMs;

    public void CompensateOnce(TimeSpan offset)
    {
        if (IsCompensated)
            throw new InvalidOperationException("Object already compensated.");

        IsCompensated = true;

        //a 5th-percentile round trip with no margin: the server keeps no grace, so a quicker leg is refused
        Offset += offset;
    }

    /// <summary>
    ///     Does nothing: <see cref="Elapsed" /> reads the clock, so a cooldown ends on time rather than at the client's next
    ///     update, which runs only every 33ms.
    /// </summary>
    /// <param name="delta">Unused.</param>
    public void Update(TimeSpan delta) { }

    /// <summary>Whether or not the skill can be used.</summary>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the skill can be used, otherwise <c>false</c> .
    /// </returns>
    public bool CanUse() => Elapsed.TotalMilliseconds > CooldownMs;
}