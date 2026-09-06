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
            if (Pathfinder.GetNavMesh("main") is null)
                Pathfinder.Initialize();
        } finally
        {
            Sync.Release();
        }
    }
}