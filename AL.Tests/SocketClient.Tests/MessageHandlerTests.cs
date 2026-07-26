#region
using System;
using System.Threading.Tasks;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Common.Logging;
using FluentAssertions;
using SocketIOClient;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_MESSAGE_HANDLER)]
public class MessageHandlerTests
{
    public static ALSocketClient Socket { get; set; } = null!;

    [After(Class)]
    public static async Task Cleanup() => await Socket.DisposeAsync();

    [Test]
    public void CreateLambdaTest()
    {
        static Func<SocketIOResponse, int, object> InternalCreateLambda<T>() => ALSocketClient.CreateLambda(typeof(T));

        var lambda = InternalCreateLambda<SlotItem[]>();

        lambda.Should()
              .NotBeNull();
    }

    [Test]
    public async Task HandleMessageTest()
    {
        var source = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            obj =>
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                var result = obj != null;
                source.TrySetResult(result);

                return source.Task;
            });

        await Socket.HandleEventAsync(
            @"[
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

        (await source.Task).Should()
                           .BeTrue();
    }

    [Before(Class)]
    public static void Init() => Socket = new ALSocketClient(new FormattedLogger("test", LogManager.GetLogger<ALSocketClient>()));
}