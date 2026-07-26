namespace AL.Core.Interfaces;

/// <summary>
///     Implemented by a type whose
///     <c>
///         stat
///     </c>
///     wire key may arrive as a scroll-stat NAME (a string) instead of a numeric stat value. System.Text.Json cannot
///     resume binding after a failed member the way Newtonsoft's
///     <c>
///         [OnError]
///     </c>
///     did, so the attributed-object converter strips a non-numeric
///     <c>
///         stat
///     </c>
///     before binding (its numeric target would throw and abort the whole object) and hands the name here for recovery.
/// </summary>
public interface IScrollStatRecoverable
{
    /// <summary>
    ///     Records the scroll stat named by a non-numeric
    ///     <c>
    ///         stat
    ///     </c>
    ///     wire value.
    /// </summary>
    void RecoverScrollStat(string statName);
}