using System.Threading;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Data;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests
{
    [TestClass]
    public abstract class GameDataTestBed
    {
        protected static readonly SemaphoreSlim Sync = new(1, 1);   
        
        [TestInitialize]
        public virtual async Task Init()
        {
            await Sync.WaitAsync();

            try
            {
                if (GameData.Version == 0)
                    GameData.Populate(await ALAPIClient.GetGameDataAsync().ConfigureAwait(false));
            } finally
            {
                Sync.Release();
            }
        }
    }
}