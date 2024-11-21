#region
using System.Threading.Tasks;
using AL.Pathfinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests;

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
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Pathfinder.DirectedGraph == null)
                Pathfinder.Initialize();
        } finally
        {
            Sync.Release();
        }
    }
}