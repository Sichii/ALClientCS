#region
using AL.Data;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

public class JsonConverterTests : GameDataTestBed
{
    [Test]
    public void GameDataTest()
    {
        GameData.Classes
                .Should()
                .NotBeNull();

        GameData.Achievements
                .Should()
                .NotBeNull();

        GameData.Maps
                .Should()
                .NotBeNull();
    }
}