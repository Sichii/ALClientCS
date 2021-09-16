using System.Threading;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests
{
    [TestClass]
    public abstract class ClientTestBed : APITestBed
    {
        [TestInitialize]
        public async Task Init()
        {
            await Sync.WaitAsync();

            try
            {

            } finally
            {
                Sync.Release();
            }
        }
    }
}