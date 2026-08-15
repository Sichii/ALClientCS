#region
using System.Collections.Concurrent;
using Chaos.Time.Abstractions;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Snapshot-free walks of a <see cref="ConcurrentDictionary{TKey,TValue}" />.
/// </summary>
public static class ConcurrentDictionaryExtensions
{
    /// <summary>
    ///     Ticks every value, then drops entries for which <paramref name="expired" /> is true, without taking a snapshot.
    /// </summary>
    /// <remarks>
    ///     LINQ <c>ToList</c> on a <see cref="ConcurrentDictionary{TKey,TValue}" /> reads <c>Count</c> then
    ///     <c>CopyTo</c> as two operations. A writer between them throws <see cref="ArgumentException" />. That is the
    ///     hazard on the <c>ICollection&lt;KeyValuePair&gt;</c> path only - <c>Values</c> and the dictionary's own
    ///     <c>ToArray</c> build their copy under every lock and were never at risk. The enumerator's contract is
    ///     concurrent-safe.
    ///     <br />
    ///     The trade for dropping the snapshot: the enumerator is live, so an entry written after the walk started may
    ///     be visited by it and charged a <paramref name="delta" /> it did not spend. A snapshot skipped such an entry
    ///     entirely. The error is bounded by one frame per affected entry, and taking a fresh snapshot to avoid it
    ///     costs every entity every update, which is the trade this makes deliberately.
    ///     <br />
    ///     <c>TryRemove(kvp)</c> compares the value with <see cref="EqualityComparer{T}.Default" />, so what it
    ///     protects depends on <typeparamref name="TValue" />. A class with reference equality - a
    ///     <c>CooldownInfo</c> - drops only the instance ticked here, leaving a replacement written this frame alone.
    ///     A <b>record</b> compares by value, so a replacement that happens to be equal is dropped too. That is
    ///     currently harmless for both record callers, and only because neither writes into a live map: an entity's
    ///     conditions are replaced wholesale on update, and a projectile id is unique per shot. Anything that starts
    ///     writing single entries into one of those maps needs a key-and-instance removal instead.
    /// </remarks>
    public static void TickAndTryRemoveWhere<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> source,
        TimeSpan delta,
        Func<TValue, bool> expired)
        where TKey : notnull
        where TValue : IDeltaUpdatable
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(expired);

        foreach (var kvp in source)
        {
            kvp.Value.Update(delta);

            if (expired(kvp.Value))
                source.TryRemove(kvp);
        }
    }

    /// <summary>
    ///     Drops entries for which <paramref name="predicate" /> is true, without taking a snapshot.
    /// </summary>
    /// <remarks>
    ///     Same Count-then-CopyTo hazard as <see cref="TickAndTryRemoveWhere{TKey,TValue}" />. This one ticks nothing,
    ///     so an entry written mid-walk is only tested against <paramref name="predicate" /> and not mutated.
    /// </remarks>
    public static void TryRemoveWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> source, Func<TValue, bool> predicate)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var kvp in source)
            if (predicate(kvp.Value))
                source.TryRemove(kvp);
    }
}
