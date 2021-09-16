using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Tests.Client.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Integration
{
    //[TestClass]
    public class IntegrationTests : ClientTestBed
    {
        [TestMethod]
        public async Task IdleTest()
        {

            await using var client = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, APIClient).ConfigureAwait(false);

            //stand still for 1minute
            await Task.Delay(1000 * 60).ConfigureAwait(false);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task PathfindTest()
        {
            var client = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, APIClient).ConfigureAwait(false);

            var location1 = new Location("main", -917, 133);
            var location2 = new Location("winterland", 285, -115);

            await client.SmartMoveAsync(location1).ConfigureAwait(false);
            await client.SmartMoveAsync(location2).ConfigureAwait(false);

            //we made it yay
            Assert.IsTrue(client.Character.DistanceWithMapCheck(location2) < 10);
        }
    }
}