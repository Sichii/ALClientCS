#region
using System.Diagnostics;
using AL.SocketClient.Definitions;
#endregion

namespace AL.Client.Managers;

/// <param name="Name">
///     A source label, or an emit type, depending on which grouping the row came from.
/// </param>
public sealed record CallBudgetRow(string Name, double Cost, int Emits);

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
            Group(window, entry => entry.Emit.ToString()));
    }

    private static IReadOnlyList<CallBudgetRow> Group(IReadOnlyList<Entry> window, Func<Entry, string> key)
        => window.GroupBy(key)
                 .Select(group => new CallBudgetRow(group.Key, group.Sum(entry => entry.Cost), group.Count()))
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
