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

    /// <summary>
    ///     The minimum offset value based on ping times.
    /// </summary>
    internal TimeSpan MinimumOffset { get; private set; }

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

        if (MinimumOffset == TimeSpan.Zero)
            MinimumOffset = elapsed;

        //if CyclicBuffer is not full, we keep the smallest of values until it's full
        if (!discarded.HasValue)
            MinimumOffset = new TimeSpan(Math.Min(elapsed.Ticks, MinimumOffset.Ticks));

        //if the buffer is full, and elapsed is less than the minimum in the buffer
        //we update the minimum
        else if (elapsed < MinimumOffset)
            MinimumOffset = elapsed;

        //if the buffer is full and the discarded value is the minimum
        //we know we need to recalculate the minimum
        else if (discarded.Value == MinimumOffset)
            MinimumOffset = Pings.Min()!.Value;
    }
}