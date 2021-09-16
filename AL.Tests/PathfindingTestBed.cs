using System.Threading;
using System.Threading.Tasks;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests
{
    [TestClass]
    public abstract class PathfindingTestBed : GameDataTestBed
    {
        [TestInitialize]
        public virtual async Task Init()
        {
            await base.Init();
            
            await Sync.WaitAsync();

            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (Pathfinder2.DirectedGraph == null)
                    await Pathfinder2.InitializeAsync().ConfigureAwait(false);
            } finally
            {
                Sync.Release();
            }
        }
    }
}