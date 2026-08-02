#region
using System.Diagnostics;
using AL.Client.Abstractions;
using AL.Core.Collections;
#endregion

namespace AL.Client.Managers;

public sealed class PingManager : AsyncDeltaLoop
{
    private readonly CyclicBuffer<TimeSpan?> Pings;
    public long PingCount;

    //raw measurement; TimeSpan.Zero until the first ping lands. The property floors that state instead, so the
    //update logic below may only read this field, never the property
    private TimeSpan MeasuredMinimum;

    /// <summary>
    ///     What an unmeasured connection reads as. Before the first ping a zero minimum zeroes every grace period
    ///     and compensation scaled by it - the desync detector fired on the first frame of every boot-time walk for
    ///     exactly that reason.
    /// </summary>
    private static readonly TimeSpan UNMEASURED_FLOOR = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     The smallest round trip seen in the window - the best case, which the position compensation is scaled by.
    /// </summary>
    internal TimeSpan MinimumOffset => MeasuredMinimum == TimeSpan.Zero ? UNMEASURED_FLOOR : MeasuredMinimum;

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    protected override float PollingRate { get; } = 1f / 4f; //once per 4 seconds

    internal PingManager(ALClient client)
        : base(client)
        => Pings = new CyclicBuffer<TimeSpan?>(50);

    protected override async Task DoWorkAsync(TimeSpan delta, CancellationToken cancellationToken)
    {
        var ts = Stopwatch.GetTimestamp();
        await Client.PingAsync(Interlocked.Increment(ref PingCount));
        var elapsed = Stopwatch.GetElapsedTime(ts);

        var discarded = Pings.Add(elapsed);

        if (MeasuredMinimum == TimeSpan.Zero)
            MeasuredMinimum = elapsed;

        //if CyclicBuffer is not full, we keep the smallest of values until it's full
        if (!discarded.HasValue)
            MeasuredMinimum = new TimeSpan(Math.Min(elapsed.Ticks, MeasuredMinimum.Ticks));

        //if the buffer is full, and elapsed is less than the minimum in the buffer
        //we update the minimum
        else if (elapsed < MeasuredMinimum)
            MeasuredMinimum = elapsed;

        //if the buffer is full and the discarded value is the minimum
        //we know we need to recalculate the minimum
        else if (discarded.Value == MeasuredMinimum)
            MeasuredMinimum = Pings.Min()!.Value;
    }
}
