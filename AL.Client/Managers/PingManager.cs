#region
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Client.Abstractions;
using AL.Core.Collections;
using AL.Core.Helpers;
using Chaos.Time;
#endregion

namespace AL.Client.Managers;

public sealed class PingManager : AsyncDeltaLoop
{
    private readonly CyclicBuffer<TimeSpan?> Pings;
    public long PingCount;

    /// <summary>
    ///     The minimum offset value based on ping times.
    /// </summary>
    internal TimeSpan Offset { get; private set; }

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

        if (!discarded.HasValue)
            Offset = new TimeSpan(Math.Min(elapsed.Ticks, Offset.Ticks));
        else if (discarded.Value == Offset)
            Offset = Pings.Min()!.Value;
    }
}