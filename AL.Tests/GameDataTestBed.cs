#region
using System.Threading.Tasks;
using AL.APIClient;
using AL.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests;

[TestClass]
public abstract class GameDataTestBed : APITestBed
{
    [TestInitialize]
    public override async Task Init()
    {
        await base.Init();

        await Sync.WaitAsync();

        try
        {
            if (GameData.Version == 0)
                GameData.Populate(await ALAPIClient.GetGameDataAsync());
        } finally
        {
            Sync.Release();
        }
    }
}