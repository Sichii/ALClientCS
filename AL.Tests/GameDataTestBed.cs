#region
using AL.APIClient;
using AL.Data;
#endregion

namespace AL.Tests;

[NotInParallel(ParallelKeys.GAME_DATA)]
public abstract class GameDataTestBed : APITestBed
{
    [Before(Test)]
    public async Task LoadGameDataAsync()
    {
        await Sync.WaitAsync();

        try
        {
            if (GameData.Version == 0)
                GameData.Populate(await AlApiClient.GetGameDataAsync());
        } finally
        {
            Sync.Release();
        }
    }
}