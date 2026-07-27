#region
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using SocketIOClient;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_MESSAGE_HANDLER)]
public class MessageHandlerTests : SocketTestBed
{
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

        await Socket.HandleEventAsync(ACTION_FRAME);

        (await source.Task).Should()
                           .BeTrue();
    }
}