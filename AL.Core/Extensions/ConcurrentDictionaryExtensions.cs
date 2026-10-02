#region
using System.Collections.Concurrent;
using Chaos.Time.Abstractions;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides removal walks over a <see cref="ConcurrentDictionary{TKey,TValue}" /> that take no snapshot.
/// </summary>
public static class ConcurrentDictionaryExtensions
{
    /// <summary>
    ///     Updates every value by <paramref name="delta" />, then removes the entries <paramref name="isExpiredFunc" />
    ///     reports expired.
    /// </summary>
    /// <remarks>
    ///     The walk is live, so an entry written after it started may be updated too. <c>TryRemove(kvp)</c> compares values
    ///     by default equality, so for a record value a replacement equal to the expired entry is removed as well.
    /// </remarks>
    /// <param name="source">
    ///     The dictionary to walk.
    /// </param>
    /// <param name="delta">
    ///     The time elapsed since the last update.
    /// </param>
    /// <param name="isExpiredFunc">
    ///     Determines whether an updated value should be removed.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     source
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     isExpiredFunc
    /// </exception>
    public static void UpdateAndTryRemoveWhere<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> source,
        TimeSpan delta,
        Func<TValue, bool> isExpiredFunc) where TKey: notnull
                                          where TValue: IDeltaUpdatable
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentNullException.ThrowIfNull(isExpiredFunc);

        foreach (var kvp in source)
        {
            kvp.Value.Update(delta);

            if (isExpiredFunc(kvp.Value))
                source.TryRemove(kvp);
        }
    }

    /// <summary>
    ///     Removes the entries whose value matches <paramref name="predicate" />.
    /// </summary>
    /// <param name="source">
    ///     The dictionary to walk.
    /// </param>
    /// <param name="predicate">
    ///     Determines whether a value should be removed.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     source
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     predicate
    /// </exception>
    public static void TryRemoveWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, Func<TValue, bool> predicate)
        where TKey: notnull
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var kvp in source)
            if (predicate(kvp.Value))
                source.TryRemove(kvp);
    }
}