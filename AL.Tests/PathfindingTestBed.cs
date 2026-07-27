#region
using AL.Pathfinding;
#endregion

namespace AL.Tests;

public abstract class PathfindingTestBed : GameDataTestBed
{
    [Before(Test)]
    public async Task InitializePathfinderAsync()
    {
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