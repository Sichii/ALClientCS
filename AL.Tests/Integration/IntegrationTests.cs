using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Extensions;
using AL.Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Integration
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public async Task IdleTest()
        {
            var apiClient = AssemblyInit.APIClient;

            await using var client = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, apiClient).ConfigureAwait(false);

            //stand still for 1minute
            await Task.Delay(1000 * 60).ConfigureAwait(false);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task PathfindTest()
        {
            var apiClient = AssemblyInit.APIClient;

            var client = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, apiClient).ConfigureAwait(false);

            var location1 = new Location("main", -917, 133);
            var location2 = new Location("winterland", 285, -115);

            await client.SmartMoveAsync(location1).ConfigureAwait(false);
            await client.SmartMoveAsync(location2).ConfigureAwait(false);

            //we made it yay
            Assert.IsTrue(client.Character.Distance(location2) < 10);
        }
    }
}