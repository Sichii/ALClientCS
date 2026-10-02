#region
using System.Diagnostics;
using AL.Client.Abstractions;
#endregion

namespace AL.Client.Managers;

public sealed class PingManager : AsyncDeltaLoop
{
    /// <summary>
    ///     The offset an unmeasured connection reads as, so grace periods and compensation scaled by it are not zero before
    ///     the first ping.
    /// </summary>
    private static readonly TimeSpan UNMEASURED_FLOOR = TimeSpan.FromMilliseconds(100);

    /// <summary>
    ///     The percentile of the window <see cref="LowPercentileOffset" /> reads, low enough to stay conservative but not
    ///     pinned by one unusually fast ping the way a minimum is.
    /// </summary>
    private const double OFFSET_PERCENTILE = 5d;

    /// <summary>
    ///     The number of pings the window holds, the last 200 seconds at one every 4 seconds.
    /// </summary>
    private const int WINDOW_SIZE = 50;

    public long PingCount;

    /// <summary>
    ///     The measured round trips in the window, oldest first, empty until the first ping lands.
    /// </summary>
    /// <remarks>
    ///     Replaced whole on every ping rather than written in place, so a reader on another thread always holds a complete
    ///     window.
    /// </remarks>
    internal IReadOnlyList<TimeSpan> History { get; private set; } = [];

    /// <summary>
    ///     A fast round trip for this connection: the <see cref="OFFSET_PERCENTILE" />th percentile of the window, or 100ms
    ///     until the first ping lands. Entity positions, cooldown compensation and the correction grace all scale off it.
    /// </summary>
    /// <remarks>
    ///     Below 21 samples the nearest rank is index 0, so a partly filled window reads as its minimum.
    /// </remarks>
    internal TimeSpan LowPercentileOffset
    {
        get => field == TimeSpan.Zero ? UNMEASURED_FLOOR : field;

        private set;
    }

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    protected override float PollingRate { get; } = 1f / 4f; //once per 4 seconds

    internal PingManager(ALClient client)
        : base(client) { }

    protected override async Task DoWorkAsync(TimeSpan delta, CancellationToken cancellationToken)
    {
        var ts = Stopwatch.GetTimestamp();
        await Client.PingAsync(Interlocked.Increment(ref PingCount));
        var elapsed = Stopwatch.GetElapsedTime(ts);

        History =
        [
            .. History.TakeLast(WINDOW_SIZE - 1),
            elapsed
        ];

        LowPercentileOffset = CalculatePercentile(History, OFFSET_PERCENTILE);
    }

    /// <summary>
    ///     Calculates the round trip at the given percentile of <paramref name="samples" />, by nearest rank.
    /// </summary>
    /// <param name="samples">
    ///     The measured round trips, in any order.
    /// </param>
    /// <param name="percentile">
    ///     Where to read in the sorted samples, from 0 to 100, at rank <c>ceil(percentile / 100 * count) - 1</c> clamped into
    ///     the samples.
    /// </param>
    /// <returns>
    ///     The sample at that rank, or <see cref="TimeSpan.Zero" /> if nothing has been measured yet.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     samples
    /// </exception>
    public static TimeSpan CalculatePercentile(IEnumerable<TimeSpan> samples, double percentile)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var sorted = samples.Order()
                            .ToArray();

        if (sorted.Length == 0)
            return TimeSpan.Zero;

        var rank = (int)Math.Ceiling(percentile / 100d * sorted.Length) - 1;

        return sorted[Math.Clamp(rank, 0, sorted.Length - 1)];
    }
}