using System.Threading.Tasks;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests
{
    [TestClass]
    public abstract class PathfindingTestBed : GameDataTestBed
    {
        [TestInitialize]
        public override async Task Init()
        {
            await base.Init();

            await Sync.WaitAsync();

            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (Pathfinder.DirectedGraph == null)
                    await Pathfinder.InitializeAsync().ConfigureAwait(false);
            } finally
            {
                Sync.Release();
            }
        }
    }
}