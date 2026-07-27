#region
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Extensions;
using AL.Core.Geometry;
using FluentAssertions;
#endregion

namespace AL.Tests.Integration;

//these drive a real character against the live server and IdleTest alone sleeps a minute, so they are
//run by hand, never in the suite. Under MSTest that was done by commenting out [TestClass]; TUnit
//discovers by [Test] alone, so the exclusion has to be explicit or they silently start running.
[Skip("manual integration test - logs in to the live server")]
public class IntegrationTests : PathfindingTestBed
{
    [Test]
    public async Task IdleTest()
    {
        await using var client = await Warrior.StartAsync(
            "makiz",
            ServerRegion.US,
            ServerId.III,
            APIClient);

        //stand still for 1minute
        await Task.Delay(1000 * 60);

        true.Should()
            .BeTrue();
    }

    [Test]
    public async Task PathfindTest()
    {
        var client = await Warrior.StartAsync(
            "makiz",
            ServerRegion.US,
            ServerId.III,
            APIClient);

        var location1 = new Location("main", -917, 133);
        var location2 = new Location("winterland", 285, -115);

        await client.SmartMoveAsync(location1);
        await client.SmartMoveAsync(location2);

        //we made it yay
        (client.Character.DistanceWithMapCheck(location2) < 10).Should()
                                                               .BeTrue();
    }
}