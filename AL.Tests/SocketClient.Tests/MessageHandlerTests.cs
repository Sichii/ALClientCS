#region
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using SocketIO.Core;
using SocketIO.Serializer.SystemTextJson;
using SocketIOClient;
#endregion

namespace AL.Tests.SocketClient.Tests;

[NotInParallel(ParallelKeys.SOCKET_MESSAGE_HANDLER)]
public class MessageHandlerTests : SocketTestBed
{
    //the wire assumption SendPartyInviteAsync's game_response arm rests on: three of the four server party/invite
    //paths answer with one of these and never emit the "Invited X to party" game_log the success arm waits for
    //(node/server.js:10914-10929), so every invite to someone offline, already partied, or over the cap used to
    //cost a full network timeout. If a code or the place ever drifts, that silent 1500ms stall comes back.
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

    private static readonly string[] PARTY_REFUSAL_FRAMES =
    [
        @"[""game_response"",{""response"":""invalid"",""place"":""party"",""failed"":true}]",
        @"[""game_response"",{""response"":""party_full"",""place"":""party"",""failed"":true}]",
        @"[""game_response"",{""response"":""already_in_party"",""place"":""party"",""success"":true}]"
    ];

    private const string ANCHORLESS_DISAPPEARING_TEXT_FRAME =
        @"[""disappearing_text"",{""message"":""+1234"",""x"":595.7,""y"":1091.1,""args"":{""color"":""+gold"",""size"":""large""}}]";

    //OnAny binds against the message the transport already parsed, reached through a private field, because every
    //public route back to it re-serializes the frame first. A library bump that renames that field, or stops
    //carrying the parsed array, would otherwise drop every frame at runtime with nothing to read but the drop line
    [Test]
    public void ResponseCarriesAReadableMessage()
    {
        var serializer = new SystemTextJsonSerializer(SocketJson.Options);
        var message = serializer.Deserialize(EngineIO.V4, $"42{ACTION_FRAME}");

        message.Should()
               .NotBeNull();

        var response = new SocketIOResponse(message!, null!);

        ALSocketClient.MessageOf(response)
                      .Should()
                      .BeSameAs(message);

        message.Should()
               .BeOfType<JsonMessage>()
               .Which
               .JsonArray
               .Should()
               .NotBeNullOrEmpty();
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