#region
using System;
using System.Threading.Tasks;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

[TestClass]
public class DispatchTests
{
    private const string ACTION_FRAME = @"[
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
]";

    public static ALSocketClient Socket { get; set; } = null!;

    [TestMethod]
    public async Task HandledSubscriberStillShortCircuitsTest()
    {
        var laterRan = false;

        using var handler = Socket.On<ActionData>(ALSocketMessageType.Action, _ => Task.FromResult(true));

        using var later = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            _ =>
            {
                laterRan = true;

                return Task.FromResult(false);
            });

        await Socket.HandleEventAsync(ACTION_FRAME);

        Assert.IsFalse(laterRan);
    }

    [TestMethod]
    public async Task ThrowingSubscriberDoesNotStarveLaterSubscribersTest()
    {
        var laterRan = false;

        using var thrower = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            _ => throw new InvalidOperationException("subscriber blew up"));

        using var later = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            _ =>
            {
                laterRan = true;

                return Task.FromResult(false);
            });

        await Socket.HandleEventAsync(ACTION_FRAME);

        Assert.IsTrue(laterRan);
    }

    [ClassInitialize]
    public static void Init(TestContext context)
        => Socket = new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>()));
}
