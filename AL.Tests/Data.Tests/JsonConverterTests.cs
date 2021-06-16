using AL.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Data.Tests
{
    [TestClass]
    public class JsonConverterTests
    {
        [TestMethod]
        public void GameDataTest() => Assert.IsNotNull(GameData.Classes);
    }
}