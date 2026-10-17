#region
using AL.Client.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

public class CooldownInfoTests
{
    [Test]
    public async Task ACooldownEndsOnTheClock()
    {
        var cooldown = new CooldownInfo(30);

        cooldown.CanUse()
                .Should()
                .BeFalse();

        //no Update call: the client's 33ms update loop is not what ends it
        await Task.Delay(60);

        cooldown.CanUse()
                .Should()
                .BeTrue();

        cooldown.RemainingMS
                .Should()
                .BeLessThan(0);
    }

    [Test]
    public void CompensationCountsAsTimeGone()
    {
        var cooldown = new CooldownInfo(1000);

        cooldown.CompensateOnce(TimeSpan.FromMilliseconds(105));

        cooldown.RemainingMS
                .Should()
                .BeInRange(880, 895);
    }
}