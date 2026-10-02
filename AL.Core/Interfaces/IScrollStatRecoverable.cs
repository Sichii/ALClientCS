namespace AL.Core.Interfaces;

/// <summary>
///     Represents a type whose <c>stat</c> wire key may arrive as a scroll stat name instead of a number.
/// </summary>
/// <remarks>
///     The attributed-object converter strips a non-numeric <c>stat</c> before binding, since it would fail the whole
///     object, and passes the name here.
/// </remarks>
public interface IScrollStatRecoverable
{
    /// <summary>
    ///     Records the scroll stat named by a non-numeric <c>stat</c> wire value.
    /// </summary>
    /// <param name="statName">The scroll stat's name.</param>
    void RecoverScrollStat(string statName);
}