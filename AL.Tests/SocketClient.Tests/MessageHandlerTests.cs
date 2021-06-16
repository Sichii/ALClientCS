using System.Threading.Tasks;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Extensions;
using AL.SocketClient.Receive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.SocketClient.Tests
{
    [TestClass]
    public class MessageHandlerTests
    {
        public static ALSocketClient Socket { get; set; }

        [TestMethod]
        public async Task HandleMessageTest()
        {
            var source = new TaskCompletionSource<bool>();

            await using var subscription = Socket.On<ActionData>(ALSocketMessageType.Action, obj =>
            {
                var result = obj != null;
                source.TrySetResult(result);
                return source.Task;
            });

            await Socket.HandleMessageAsync(@"[
   ""action"",
   {
      ""attacker"":""2144160"",
      ""target"":""Moneybaggers"",
      ""type"":""attack"",
      ""source"":""attack"",
      ""x"":595.7417319170224,
      ""y"":1091.179435638155,
      ""eta"":400,
      ""m"":361,
      ""pid"":""wMhQBT"",
      ""projectile"":""stone"",
      ""damage"":25
   }
]");

            Assert.IsTrue(await source.Task);
        }

        [ClassInitialize]
        public static void Init(TestContext context) => Socket = new ALSocketClient(AssemblyInit.APIClient);

        [TestMethod]
        public async Task TimeoutTest()
        {
            var source = new TaskCompletionSource<bool>();

            static async Task<bool> LongTask()
            {
                await Task.Delay(2000);
                return true;
            }

            await using var subscription = Socket.On<ActionData>(ALSocketMessageType.Action, obj =>
            {
                var result = obj != null;
                source.TrySetResult(result);
                return source.Task;
            });

            Assert.IsTrue(!await LongTask().Timeout(500));
        }
    }
}