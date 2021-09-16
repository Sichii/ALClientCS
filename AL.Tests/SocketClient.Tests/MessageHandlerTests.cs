using System;
using System.Threading.Tasks;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketIOClient;

namespace AL.Tests.SocketClient.Tests
{
    [TestClass]
    public class MessageHandlerTests
    {
        public static ALSocketClient Socket { get; set; } = null!;

        [TestMethod]
        public void CreateLambdaTest()
        {
            Func<SocketIOResponse, int, object> InternalCreateLambda<T>() => ALSocketClient.CreateLambda(typeof(T));

            var lambda = InternalCreateLambda<SlotItem[]>();

            Assert.IsNotNull(lambda);
        }

        [TestMethod]
        public async Task HandleMessageTest()
        {
            var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var subscription = Socket.On<ActionData>(ALSocketMessageType.Action, obj =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                var result = obj != null;
                source.TrySetResult(result);

                return source.Task;
            });

            await Socket.HandleEventAsync(@"[
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
]")
                .ConfigureAwait(false);

            Assert.IsTrue(await source.Task.ConfigureAwait(false));
        }

        [ClassInitialize]
        public static void Init(TestContext context) => Socket =
            new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>()));
    }
}