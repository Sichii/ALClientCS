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
    ///     What an unmeasured connection reads as. Before the first ping a zero offset zeroes every grace period
    ///     and compensation scaled by it - the desync detector fired on the first frame of every boot-time walk for
    ///     exactly that reason.
    /// </summary>
    private static readonly TimeSpan UNMEASURED_FLOOR = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     Which percentile of the window <see cref="LowPercentileOffset" /> reads. Shorter than 95% of round trips,
    ///     so still deliberately conservative, but not pinned for the window's whole 200 seconds by one freak-fast
    ///     ping the way a minimum is.
    /// </summary>
    private const double OFFSET_PERCENTILE = 5d;

    /// <summary>
    ///     A fast round trip for this connection: the <see cref="OFFSET_PERCENTILE" />th percentile of the last fifty
    ///     pings, floored at 100ms until the first one lands. One number for the whole client - entity positions are
    ///     advanced by it, cooldowns are compensated by it, and the correction grace is scaled off it.
    /// </summary>
    /// <remarks>
    ///     Below 21 samples its nearest rank is index 0, so a partly filled window reads as its own minimum and this
    ///     only loosens once there is a window to speak of.
    /// </remarks>
    internal TimeSpan LowPercentileOffset
    {
        get => field == TimeSpan.Zero ? UNMEASURED_FLOOR : field;

        private set;
    }

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    protected override float PollingRate { get; } = 1f / 4f; //once per 4 seconds

    internal PingManager(ALClient client)
        : base(client)
        => Pings = new CyclicBuffer<TimeSpan?>(50);

    /// <summary>
    ///     The round trip at the given percentile of <paramref name="samples" />, by nearest rank.
    /// </summary>
    /// <param name="samples">
    ///     The measured round trips, in any order. Nulls are unfilled buffer slots and are skipped.
    /// </param>
    /// <param name="percentile">
    ///     Where to read in the sorted samples, from 0 to 100. The rank taken is
    ///     <c>ceil(percentile / 100 * count) - 1</c>, clamped into the array, so 0 gives the smallest sample and any
    ///     percentile whose rank rounds below the first sample gives it too.
    /// </param>
    /// <returns>
    ///     The sample at that rank, or <see cref="TimeSpan.Zero" /> if nothing has been measured yet.
    /// </returns>
    public static TimeSpan PercentileOf(IEnumerable<TimeSpan?> samples, double percentile)
    {
        var sorted = samples.Where(sample => sample.HasValue)
                            .Select(sample => sample!.Value)
                            .Order()
                            .ToArray();

        if (sorted.Length == 0)
            return TimeSpan.Zero;

        var rank = (int)Math.Ceiling(percentile / 100d * sorted.Length) - 1;

        return sorted[Math.Clamp(rank, 0, sorted.Length - 1)];
    }

    protected override async Task DoWorkAsync(TimeSpan delta, CancellationToken cancellationToken)
    {
        var ts = Stopwatch.GetTimestamp();
        await Client.PingAsync(Interlocked.Increment(ref PingCount));
        var elapsed = Stopwatch.GetElapsedTime(ts);

        Pings.Add(elapsed);

        //50 samples once every 4 seconds - a rescan costs less than being sure incremental maintenance is right
        LowPercentileOffset = PercentileOf(Pings, OFFSET_PERCENTILE);
    }
}
