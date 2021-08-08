using System.Diagnostics;
using System.Threading.Tasks;
using AL.Client.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Client.Tests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public async Task DynamicDelayTest()
        {
            var delay = new DynamicDelay();

            var delayTask = delay.WaitAsync(5000);

            await Task.Delay(2000);
            await delay.SetDelayAsync(10000);
            var timer = Stopwatch.StartNew();
            await delayTask;
            timer.Stop();

            var elapsed = timer.ElapsedMilliseconds;

            Assert.IsTrue(elapsed > 5000);
        }
    }
}