using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Integration
{
    [TestClass]
    public class IntegrationTests
    {
        //[TestMethod]
        public async Task Test()
        {
            var apiClient = AssemblyInit.APIClient;
            await PathFinder.InitializeAsync();

            var client = await ALClient.CreateAsync("makiz", ServerRegion.US, ServerId.II, apiClient);
            
            //stand still for 1minute
            await Task.Delay(1000 * 60);
            
            Assert.IsTrue(true);
        }
    }
}