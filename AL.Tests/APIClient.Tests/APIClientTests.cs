using System.Linq;
using System.Threading.Tasks;
using AL.APIClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.APIClient.Tests
{
    [TestClass]
    public class APIClientTests
    {
        private static ALAPIClient APIClient = null!;

        [TestMethod]
        public async Task GetMailTest()
        {
            var result = await APIClient.GetMailAsync().ToListAsync().ConfigureAwait(false);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetMerchantTest()
        {
            var result = await APIClient.GetMerchantsAsync().ToListAsync().ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [ClassInitialize]
        public static void Init(TestContext context) => APIClient = AssemblyInit.APIClient;

        [TestMethod]
        public async Task UpdateServersAndCharactersTest()
        {
            var serversAndCharacters = await APIClient.GetServersAndCharactersAsync().ConfigureAwait(false);

            Assert.IsNotNull(serversAndCharacters.Servers);
            Assert.IsNotNull(serversAndCharacters.Characters);
            Assert.IsTrue(serversAndCharacters.Servers.Any());
            Assert.IsTrue(serversAndCharacters.Characters.Any());
        }
    }
}