#region
using AL.Client.Managers;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     What the ping window reads as at a percentile. Entity position compensation is scaled by the 5th, and the
///     reason it is not the minimum is that one freak-fast round trip pins a minimum for the window's whole 200
///     seconds - so the last test here is the point of that choice rather than a corner case.
/// </summary>
public class PingPercentileTests
{
    //the manager's buffer hands out every slot it owns, so the ones no ping has written yet arrive as nulls
    private static TimeSpan?[] Window(IEnumerable<double> measuredMs, int size = 50)
    {
        var window = new TimeSpan?[size];
        var index = 0;

        foreach (var ms in measuredMs)
            window[index++] = TimeSpan.FromMilliseconds(ms);

        return window;
    }

    [Test]
    public void AnUnmeasuredWindowReadsAsZero()
    {
        PingManager.PercentileOf(new TimeSpan?[50], 5d)
                   .Should()
                   .Be(TimeSpan.Zero);

        PingManager.PercentileOf([], 5d)
                   .Should()
                   .Be(TimeSpan.Zero);
    }

    [Test]
    public void OneSampleIsEveryPercentile()
    {
        var window = Window([42d]);

        PingManager.PercentileOf(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));

        PingManager.PercentileOf(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));

        PingManager.PercentileOf(window, 100d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(42));
    }

    [Test]
    public void APartlyFilledWindowReadsAsItsOwnMinimum()
    {
        //nineteen samples: ceil(0.05 * 19) - 1 is index 0, the same answer a minimum gives
        var narrow = Window(Enumerable.Range(1, 19)
                                      .Select(i => (double)(i * 10)));

        PingManager.PercentileOf(narrow, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(10));

        //twenty one is where the rank first clears index 0 and the percentile starts saying something of its own
        var wider = Window(Enumerable.Range(1, 21)
                                     .Select(i => (double)(i * 10)));

        PingManager.PercentileOf(wider, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(20));
    }

    [Test]
    public void AFullWindowTakesTheThirdSmallest()
    {
        //fifty samples: ceil(0.05 * 50) - 1 is index 2
        var window = Window(Enumerable.Range(1, 50)
                                      .Select(i => (double)i));

        PingManager.PercentileOf(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(3));

        PingManager.PercentileOf(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(1));

        PingManager.PercentileOf(window, 100d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(50));
    }

    [Test]
    public void OneFreakFastSampleDoesNotDragThePercentile()
    {
        //one round trip a third of the rest, sitting at the front so nothing depends on the input being sorted
        var pings = Enumerable.Repeat(60d, 49)
                              .Prepend(20d);

        var window = Window(pings);

        PingManager.PercentileOf(window, 0d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(20));

        PingManager.PercentileOf(window, 5d)
                   .Should()
                   .Be(TimeSpan.FromMilliseconds(60));
    }
}
