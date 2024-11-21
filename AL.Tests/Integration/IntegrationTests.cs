#region
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Extensions;
using AL.Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.Integration;

//[TestClass]
public class IntegrationTests : PathfindingTestBed
{
    [TestMethod]
    public async Task IdleTest()
    {
        await using var client = await Warrior.StartAsync(
            "makiz",
            ServerRegion.US,
            ServerId.III,
            APIClient);

        //stand still for 1minute
        await Task.Delay(1000 * 60);

        Assert.IsTrue(true);
    }

    [TestMethod]
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
        Assert.IsTrue(client.Character.DistanceWithMapCheck(location2) < 10);
    }
}