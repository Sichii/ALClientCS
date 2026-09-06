#region
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using SocketIO.Core;
using SocketIO.Serializer.Core;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_MESSAGE_HANDLER)]
public class MessageHandlerTests : SocketTestBed
{
    private static readonly string[] PARTY_REFUSAL_FRAMES =
    [
        @"[""game_response"",{""response"":""invalid"",""place"":""party"",""failed"":true}]",
        @"[""game_response"",{""response"":""party_full"",""place"":""party"",""failed"":true}]",
        @"[""game_response"",{""response"":""already_in_party"",""place"":""party"",""success"":true}]"
    ];

    private const string ANCHORLESS_DISAPPEARING_TEXT_FRAME
        = @"[""disappearing_text"",{""message"":""+1234"",""x"":595.7,""y"":1091.1,""args"":{""color"":""+gold"",""size"":""large""}}]";

    //the frame that used to kill the potion/regen callback: the server anchors its gold and xp texts to a point
    //rather than an entity, so "id" is simply absent (node/server.js:2825, :2847, :10147). Id must read as null -
    //declared non-nullable it silently became a null receiver, and EqualsI throws on one
    [Test]
    public async Task AnchorlessDisappearingTextBindsNullId()
    {
        DisappearingTextData? seen = null;

        using var subscription = Socket.On<DisappearingTextData>(
            ALSocketMessageType.DisappearingText,
            data =>
            {
                seen = data;

                return Task.FromResult(false);
            });

        await Socket.HandleEventAsync(ANCHORLESS_DISAPPEARING_TEXT_FRAME);

        seen.Should()
            .NotBeNull();

        seen!.Id
             .Should()
             .BeNull();

        seen.Message
            .Should()
            .Be("+1234");
    }

    //the library dispatches every received frame on a thread-pool task of its own, so a burst reaches a handler in
    //whichever order the pool schedules it - a buy receipt used to overtake the inventory frame sent before it. The
    //parse is the one call still made inline in arrival order, and the queue is fed from there: two frames parsed
    //in sequence must reach their subscribers in that sequence
    [Test]
    public async Task FramesParsedInSequenceAreHandledInSequence()
    {
        var seen = new List<ALSocketMessageType>();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using var action = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            _ =>
            {
                seen.Add(ALSocketMessageType.Action);

                return Task.FromResult(false);
            });

        using var response = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            _ =>
            {
                seen.Add(ALSocketMessageType.GameResponse);
                done.TrySetResult();

                return Task.FromResult(false);
            });

        ISerializer serializer = new ALSocketClient.SynchronousSerializer(Socket, SocketJson.Options);

        serializer.Deserialize(EngineIO.V4, $"42{ACTION_FRAME}");
        serializer.Deserialize(EngineIO.V4, $"42{PARTY_REFUSAL_FRAMES[0]}");

        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        seen.Should()
            .Equal(ALSocketMessageType.Action, ALSocketMessageType.GameResponse);
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

    //the wire assumption SendPartyInviteAsync's game_response arm rests on: three of the four server party paths
    //answer with one of these and never emit the "Invited X to party" game_log the success arm waits for
    //(node/server.js:10914-10929), so an invite to someone offline used to cost a full network timeout
    [Test]
    public async Task PartyInviteRefusalsBindWithTheirPlace()
    {
        var seen = new List<GameResponseData>();

        using var subscription = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                seen.Add(data);

                return Task.FromResult(false);
            });

        foreach (var frame in PARTY_REFUSAL_FRAMES)
            await Socket.HandleEventAsync(frame);

        seen.Select(data => (data.ResponseType, data.Place, data.Failed))
            .Should()
            .Equal(
                (GameResponseType.Invalid, "party", true),
                (GameResponseType.PartyFull, "party", true),
                (GameResponseType.AlreadyInParty, "party", false));
    }
}