#region
using AL.Client.Managers;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     What the ping window reads as at a percentile. Entity position compensation is scaled by the 5th, and the reason it
///     is not the minimum is that one freak-fast round trip pins a minimum for the window's whole 200 seconds - so the
///     last test here is the point of that choice rather than a corner case.
/// </summary>
public class PingPercentileTests
{
    [Test]
    public void AFullWindowTakesTheThirdSmallest()
    {
        //fifty samples: ceil(0.05 * 50) - 1 is index 2
        var window = Window(
            Enumerable.Range(1, 50)
                      .Select(i => (double)i));

        PingManager.CalculatePercentile(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(3));

        PingManager.CalculatePercentile(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(1));

        PingManager.CalculatePercentile(window, 100d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(50));
    }

    [Test]
    public void APartlyFilledWindowReadsAsItsOwnMinimum()
    {
        //nineteen samples: ceil(0.05 * 19) - 1 is index 0, the same answer a minimum gives
        var narrow = Window(
            Enumerable.Range(1, 19)
                      .Select(i => (double)(i * 10)));

        PingManager.CalculatePercentile(narrow, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(10));

        //twenty one is where the rank first clears index 0 and the percentile starts saying something of its own
        var wider = Window(
            Enumerable.Range(1, 21)
                      .Select(i => (double)(i * 10)));

        PingManager.CalculatePercentile(wider, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(20));
    }

    [Test]
    public void AnUnmeasuredWindowReadsAsZero() => PingManager.CalculatePercentile([], 5d)
                                                              .Should()
                                                              .Be(TimeSpan.Zero);

    [Test]
    public void OneFreakFastSampleDoesNotDragThePercentile()
    {
        //one round trip a third of the rest, sitting at the front so nothing depends on the input being sorted
        var pings = Enumerable.Repeat(60d, 49)
                              .Prepend(20d);

        var window = Window(pings);

        PingManager.CalculatePercentile(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(20));

        PingManager.CalculatePercentile(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(60));
    }

    [Test]
    public void OneSampleIsEveryPercentile()
    {
        var window = Window([42d]);

        PingManager.CalculatePercentile(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));

        PingManager.CalculatePercentile(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));

        PingManager.CalculatePercentile(window, 100d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));
    }

    private static TimeSpan[] Window(IEnumerable<double> measuredMs)
        => measuredMs.Select(TimeSpan.FromMilliseconds)
                     .ToArray();
}