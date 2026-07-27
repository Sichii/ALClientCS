#region
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_DISPATCH)]
public class DispatchTests : SocketTestBed
{
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