#region
using System.Diagnostics;
using AL.SocketClient.Definitions;
#endregion

namespace AL.Client.Managers;

/// <summary>Represents one row of a call-budget breakdown.</summary>
/// <param name="Name">
///     A source label, or an emit type, depending on which grouping the row came from.
/// </param>
/// <param name="Cost">
///     The summed cost of every emit in this row, by <see cref="CallCost.CalculateCost" />.
/// </param>
/// <param name="Emits">
///     The number of emits this row's cost was summed from.
/// </param>
public sealed record CallBudgetRow(string Name, double Cost, int Emits)
{
    /// <summary>
    ///     The same spend broken down one level further, or empty when this row is already the finest grain asked for.
    /// </summary>
    public IReadOnlyList<CallBudgetRow> Children { get; init; } = [];
}

/// <summary>
///     Represents the call meter's window: what it cost, and how that cost breaks down.
/// </summary>
/// <param name="ServerCost">
///     The server's own accrued cost for this window, from the last <c>player</c> frame. Authoritative;
///     <paramref name="Cost" /> should agree with it to within a frame.
/// </param>
/// <param name="Cost">
///     The cost of the emits in the window, by <see cref="CallCost.CalculateCost" />, dated the way the server dates them.
/// </param>
/// <param name="Emits">The number of emits the window covers.</param>
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
    public static readonly CallBudgetSnapshot EMPTY = new(
        0d,
        0d,
        0,
        [],
        []);
}

/// <summary>
///     Represents the window the server meters calls over, entry for entry, so a character's spend can be read back broken
///     down rather than as the one number the server sends. Every emit that reaches the wire is recorded, priced or free.
/// </summary>
/// <remarks>
///     <see cref="SourceResolver" /> is called on the emitting thread, so it must be cheap; left unset, everything groups
///     under one name.
/// </remarks>
public sealed class CallMeter
{
    private const string UNATTRIBUTED = "(unattributed)";
    private readonly List<Entry> Entries = [];
    private readonly Lock Sync = new();

    /// <summary>
    ///     The function that names whatever is making the call, such as a component, a script or a task. Returning null, or
    ///     leaving this unset, files the emit under <c>(unattributed)</c>.
    /// </summary>
    public Func<string?>? SourceResolver { get; set; }

    /// <summary>
    ///     The function that reads the current stopwatch ticks, replaceable so a test can move the window.
    /// </summary>
    internal Func<long> Timestamp { get; init; } = Stopwatch.GetTimestamp;

    /// <summary>
    ///     Corrects the newest emit of this type by <paramref name="cost" />, for what the server charges on an emit's payload
    ///     rather than its name - a bank crossing, a party's chest, a potion that bills under the equip row.
    /// </summary>
    /// <remarks>
    ///     Folded into that emit's entry and never below zero. Once the entry has left the window, a charge opens a new one
    ///     and a refund is dropped.
    /// </remarks>
    /// <param name="emitType">The emit the charge belongs to.</param>
    /// <param name="cost">The cost to add, negative for a refund.</param>
    internal void Charge(ALSocketEmitType emitType, double cost)
    {
        if (cost == 0)
            return;

        var now = Timestamp();

        lock (Sync)
        {
            Prune(now);

            var index = Entries.FindLastIndex(entry => entry.Emit == emitType);

            if (index >= 0)
            {
                Entries[index] = Entries[index] with
                {
                    Cost = Math.Max(0, Entries[index].Cost + cost)
                };

                return;
            }

            if (cost > 0)
                Entries.Add(
                    new Entry(
                        now,
                        SourceResolver?.Invoke() ?? UNATTRIBUTED,
                        emitType,
                        cost));
        }
    }

    /// <summary>
    ///     Groups a window of entries into rows, one per key, each summing its group's cost.
    /// </summary>
    /// <param name="window">The entries to group into rows.</param>
    /// <param name="keySelector">
    ///     What to group each entry by - the row's <see cref="CallBudgetRow.Name" />.
    /// </param>
    /// <param name="createChildrenFunc">
    ///     Given the entries of one group, the breakdown to hang under it. Null leaves the row a leaf.
    /// </param>
    /// <returns>The rows, most expensive first.</returns>
    private static IReadOnlyList<CallBudgetRow> Group(
        IReadOnlyList<Entry> window,
        Func<Entry, string> keySelector,
        Func<IReadOnlyList<Entry>, IReadOnlyList<CallBudgetRow>>? createChildrenFunc = null)
        => window.GroupBy(keySelector)
                 .Select(group =>
                 {
                     var rows = group.ToList();

                     return new CallBudgetRow(group.Key, rows.Sum(entry => entry.Cost), rows.Count)
                     {
                         Children = createChildrenFunc?.Invoke(rows) ?? []
                     };
                 })
                 .OrderByDescending(row => row.Cost)
                 .ToList();

    /// <summary>
    ///     Drops every entry that has fallen outside the window. A free emit is dated when it happened while the run it sits
    ///     inside is dated earlier, so what has expired is not always a prefix.
    /// </summary>
    /// <param name="now">The current stopwatch ticks.</param>
    private void Prune(long now) => Entries.RemoveAll(entry => Stopwatch.GetElapsedTime(entry.Stamp, now) > CallCost.WINDOW);

    internal void Record(ALSocketEmitType emitType)
    {
        var now = Timestamp();
        var cost = CallCost.CalculateCost(emitType);
        var source = SourceResolver?.Invoke() ?? UNATTRIBUTED;

        lock (Sync)
        {
            Prune(now);

            //the server folds a charge into its last entry when the method matches and keeps that entry's date, so a
            //run of one method leaves the window as a block; a free emit neither joins a run nor ends one
            var run = Entries.FindLastIndex(entry => entry.Cost > 0);
            var stamp = (cost > 0) && (run >= 0) && (Entries[run].Emit == emitType) ? Entries[run].Stamp : now;

            Entries.Add(
                new Entry(
                    stamp,
                    source,
                    emitType,
                    cost));
        }
    }

    /// <summary>
    ///     Takes a snapshot of the window as it stands, against the server's own cost.
    /// </summary>
    /// <param name="serverCost">The character's live <c>cc</c>.</param>
    /// <returns>The window's cost and its breakdowns.</returns>
    public CallBudgetSnapshot Snapshot(double serverCost)
    {
        Entry[] window;

        lock (Sync)
        {
            Prune(Timestamp());
            window = [.. Entries];
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

            //by emit type, each row broken down by source
            Group(window, entry => entry.Emit.ToString(), inner => Group(inner, entry => entry.Source)));
    }

    private readonly record struct Entry(
        long Stamp,
        string Source,
        ALSocketEmitType Emit,
        double Cost);
}