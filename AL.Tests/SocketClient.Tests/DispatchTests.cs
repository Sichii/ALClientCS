#region
using System.Collections.Concurrent;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_DISPATCH)]
public class DispatchTests : SocketTestBed
{
    /// <summary>
    ///     Pins the ordering the town recall depends on. Handling a frame per thread pool work item lets two frames
    ///     from one burst race, and the loser can be the older one, which is what made a live recall read as
    ///     cancelled. The subscriber here is slow on purpose: under an unordered dispatch the second frame overtakes
    ///     it.
    /// </summary>
    [Test]
    public async Task FramesAreHandledInArrivalOrderTest()
    {
        var handled = new ConcurrentQueue<string>();
        var bothHandled = new TaskCompletionSource();

        using var subscriber = Socket.On<string>(
            ALSocketMessageType.Action,
            async marker =>
            {
                if (marker == "first")
                    await Task.Delay(150);

                handled.Enqueue(marker);

                if (handled.Count == 2)
                    bothHandled.TrySetResult();

                return false;
            });

        Socket.TryEnqueue(ALSocketMessageType.Action, "first", "action")
              .Should()
              .BeTrue();

        Socket.TryEnqueue(ALSocketMessageType.Action, "second", "action")
              .Should()
              .BeTrue();

        await bothHandled.Task.WaitAsync(TimeSpan.FromSeconds(5));

        handled.Should()
               .ContainInOrder("first", "second");
    }

    [Test]
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

        laterRan.Should()
                .BeFalse();
    }

    [Test]
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

        laterRan.Should()
                .BeTrue();
    }
}