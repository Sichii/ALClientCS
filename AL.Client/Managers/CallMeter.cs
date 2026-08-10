#region
using System.Diagnostics;
using AL.SocketClient.Definitions;
#endregion

namespace AL.Client.Managers;

/// <param name="Name">
///     A source label, or an emit type, depending on which grouping the row came from.
/// </param>
/// <param name="Cost">
///     The summed cost of every emit in this row, by <see cref="CallCost.Of" />.
/// </param>
/// <param name="Emits">
///     How many emits this row's cost was summed from.
/// </param>
public sealed record CallBudgetRow(string Name, double Cost, int Emits)
{
    /// <summary>
    ///     The same spend broken down one level further, or empty when this row is already the finest grain asked
    ///     for. An init property rather than a fourth positional parameter, so every existing construction site and
    ///     deconstruction of this record keeps working untouched.
    /// </summary>
    public IReadOnlyList<CallBudgetRow> Children { get; init; } = [];
}

/// <param name="ServerCost">
///     The server's own accrued cost for this window, from the last
///     <c>
///         player
///     </c>
///     frame. Authoritative. <paramref name="Cost" /> is only ever a floor, because a handler bills further work
///     this side cannot see.
/// </param>
/// <param name="Cost">
///     What the emits in the window cost at minimum, by <see cref="CallCost.Of" />.
/// </param>
/// <param name="Emits">
///     How many emits the window covers.
/// </param>
/// <param name="BySource">
///     The window's cost broken down by whatever <see cref="CallMeter.SourceResolver" /> named the caller.
/// </param>
/// <param name="ByEmitType">
///     The window's cost broken down by socket emit type, each row further broken down by source.
/// </param>
public sealed record CallBudgetSnapshot(
    double ServerCost,
    double Cost,
    int Emits,
    IReadOnlyList<CallBudgetRow> BySource,
    IReadOnlyList<CallBudgetRow> ByEmitType)
{
    public static readonly CallBudgetSnapshot EMPTY = new(0d, 0d, 0, [], []);
}

/// <summary>
///     Keeps the same sliding window the server meters over, so what a character has spent can be read back
///     broken down rather than as the one number the server sends. Every emit that reaches the wire is recorded;
///     the total here and the server's own are the same measurement taken two ways, and the gap between them is
///     the server-side work an emit provokes.
/// </summary>
/// <remarks>
///     Attribution is the consumer's business - this layer has no idea what a caller is. Set
///     <see cref="SourceResolver" /> to whatever names the code path in flight (a component, a script, a task);
///     it is called on the emitting thread, so it must be cheap, and an unset resolver simply groups everything
///     under one name.
/// </remarks>
public sealed class CallMeter
{
    private const string UNATTRIBUTED = "(unattributed)";
    private readonly Queue<Entry> Entries = new();
    private readonly Lock Sync = new();

    /// <summary>
    ///     Names whatever is making the call. Runs inline on the emit, so it wants to be a field read rather than
    ///     a lookup; returning null (or leaving this unset) files the emit under
    ///     <c>
    ///         (unattributed)
    ///     </c>
    ///     .
    /// </summary>
    public Func<string?>? SourceResolver { get; set; }

    internal void Record(ALSocketEmitType emitType)
    {
        var entry = new Entry(
            Stopwatch.GetTimestamp(),
            SourceResolver?.Invoke() ?? UNATTRIBUTED,
            emitType,
            CallCost.Of(emitType));

        lock (Sync)
        {
            Entries.Enqueue(entry);
            Prune();
        }
    }

    /// <param name="serverCost">
    ///     The character's live
    ///     <c>
    ///         cc
    ///     </c>
    ///     , passed in rather than read here so this stays independent of any entity.
    /// </param>
    public CallBudgetSnapshot Snapshot(double serverCost)
    {
        Entry[] window;

        lock (Sync)
        {
            Prune();
            window = Entries.ToArray();
        }

        if (window.Length == 0)
            return CallBudgetSnapshot.EMPTY with
            {
                ServerCost = serverCost
            };

        return new CallBudgetSnapshot(
            serverCost,
            window.Sum(entry => entry.Cost),
            window.Length,
            Group(window, entry => entry.Source),

            //nested, because "which call is expensive" and "who is making it" are one question rather than two -
            //the two flat lists cannot be crossed after the fact, since each has already summed the other away
            Group(window, entry => entry.Emit.ToString(), inner => Group(inner, entry => entry.Source)));
    }

    /// <param name="window">
    ///     The entries to group into rows.
    /// </param>
    /// <param name="key">
    ///     What to group each entry by - the row's <see cref="CallBudgetRow.Name" />.
    /// </param>
    /// <param name="children">
    ///     Given the entries of one group, the breakdown to hang under it. Null leaves the row a leaf.
    /// </param>
    private static IReadOnlyList<CallBudgetRow> Group(
        IReadOnlyList<Entry> window,
        Func<Entry, string> key,
        Func<IReadOnlyList<Entry>, IReadOnlyList<CallBudgetRow>>? children = null)
        => window.GroupBy(key)
                 .Select(
                     group =>
                     {
                         var rows = group.ToList();

                         return new CallBudgetRow(group.Key, rows.Sum(entry => entry.Cost), rows.Count)
                         {
                             Children = children?.Invoke(rows) ?? []
                         };
                     })
                 .OrderByDescending(row => row.Cost)
                 .ToList();

    //the queue is in emit order, so what has expired is always a prefix - this is the server's own
    //while(mssince(calls[0]) > 4000) calls.shift()
    private void Prune()
    {
        while ((Entries.Count > 0) && (Stopwatch.GetElapsedTime(Entries.Peek().Stamp) > CallCost.WINDOW))
            Entries.Dequeue();
    }

    private readonly record struct Entry(long Stamp, string Source, ALSocketEmitType Emit, double Cost);
}
