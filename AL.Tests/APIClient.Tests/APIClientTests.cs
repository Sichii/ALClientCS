using System.Linq;
using System.Threading.Tasks;
using AL.APIClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.APIClient.Tests
{
    [TestClass]
    public class APIClientTests
    {
        private static ALAPIClient APIClient;
        
        [ClassInitialize]
        public static void Init(TestContext context) => APIClient = AssemblyInit.APIClient;

        [TestMethod]
        public async Task GetMailTest()
        {
            var result = await APIClient.GetMailAsync().ToListAsync();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetMerchantTest()
        {
            var result = await APIClient.GetMerchantsAsync().ToListAsync();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
        
        [TestMethod]
        public async Task UpdateServersAndCharactersTest()
        {
            await APIClient.UpdateServersAndCharactersAsync();

            Assert.IsNotNull(APIClient.Servers);
            Assert.IsNotNull(APIClient.Characters);
            Assert.IsTrue(APIClient.Servers.Any());
            Assert.IsTrue(APIClient.Characters.Any());
        }
    }
}