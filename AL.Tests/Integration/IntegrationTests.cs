using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Integration
{
    [TestClass]
    public class IntegrationTests
    {
        //[TestMethod]
        public async Task IdleTest()
        {
            var apiClient = AssemblyInit.APIClient;

            await using var client = await ALClient.StartCharacterAsync("makiz", ServerRegion.US, ServerId.III, apiClient);

            //stand still for 1minute
            await Task.Delay(1000 * 60);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task PathfindTest()
        {
            var apiClient = AssemblyInit.APIClient;

            await using var client = await ALClient.StartCharacterAsync("makiz", ServerRegion.US, ServerId.III, apiClient);

            var location1 = new Location("main", -917, 133);
            var location2 = new Location("winterland", 285, -115);

            await client.SmartMoveAsync(location1);
            await client.SmartMoveAsync(location2);

            //we made it yay
            Assert.IsTrue(true);
        }
    }
}