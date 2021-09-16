using AL.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Data.Tests
{
    [TestClass]
    public class JsonConverterTests : GameDataTestBed
    {
        [TestMethod]
        public void GameDataTest()
        {
            Assert.IsNotNull(GameData.Classes);
            Assert.IsNotNull(GameData.Achievements);
            Assert.IsNotNull(GameData.Maps);
        }
    }
}