#region
using System;
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
        Elapsed += minimumOffset;
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