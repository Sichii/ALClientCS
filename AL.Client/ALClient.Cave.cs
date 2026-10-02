#region
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.Data.Maps;
using AL.Pathfinding;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

/// <summary>
///     Provides the Cave of Many Dreams, the daily dungeon. Its floors stream over <c>map_chunk</c> rather than sitting in
///     G, its state rides the <c>cave</c> event onto <see cref="SocketClient.Model.Character.Cave" />, and every request
///     is an <c>interaction</c> of type <c>cave</c>, answered on <c>game_response</c> under the request id.
/// </summary>
public abstract partial class ALClient
{
    private const int CAVE_REQUEST_TIMEOUT_MS = 10_000;

    private readonly MapChunkAssembler MapChunks = new();

    /// <summary>
    ///     Asynchronously buys the one item a merchant encounter offers, from the party's purse. The character has to stand
    ///     near enough (<see cref="CaveShop.Nearby" />) and the item can be bought once.
    /// </summary>
    /// <param name="room">The room the merchant stands in.</param>
    /// <returns>The server's reply.</returns>
    /// <exception cref="ArgumentNullException">room</exception>
    public Task<GameResponseData> CaveBuyAsync(string room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return SendCaveRequestAsync(
            "buy",
            new Dictionary<string, object?>
            {
                ["room"] = room
            });
    }

    /// <summary>
    ///     Asynchronously asks the keeper to pull the party in. The party has to stand near him on main, out of combat, and
    ///     the account needs its visit for the day.
    /// </summary>
    /// <returns>The server's reply, once the run has started.</returns>
    public Task<GameResponseData> CaveEnterAsync()
    {
        //the game's own client waits this long, because the server settles a whole party's entry or vote before it answers
        const int CAVE_ENTER_TIMEOUT_MS = 150_000;

        return SendCaveRequestAsync("enter", null, CAVE_ENTER_TIMEOUT_MS);
    }

    /// <summary>
    ///     Asynchronously leaves the run for good. Works anywhere inside, fallen or mid-vote.
    /// </summary>
    /// <returns>The server's reply.</returns>
    public Task<GameResponseData> CaveExitAsync() => SendCaveRequestAsync("exit");

    /// <summary>
    ///     Asynchronously gets the account's standing with the dungeon: whether it can enter now, and when the visit comes
    ///     back if not.
    /// </summary>
    /// <returns>The account's visit.</returns>
    /// <exception cref="InvalidOperationException">The reply carried no visit.</exception>
    public async Task<CaveVisit> CaveInfoAsync()
    {
        var reply = await SendCaveRequestAsync("info");

        return reply.Visit ?? throw new InvalidOperationException("Cave info reply carried no visit.");
    }

    /// <summary>
    ///     Asynchronously chats with a passing traveler. Pauses nothing and starts no vote.
    /// </summary>
    /// <param name="room">The room the traveler stands in.</param>
    /// <param name="actorId">The traveler's id.</param>
    /// <returns>What the traveler said, if anything.</returns>
    /// <exception cref="ArgumentNullException">room</exception>
    /// <exception cref="ArgumentNullException">actorId</exception>
    public async Task<CaveChat?> CaveTalkAsync(string room, string actorId)
    {
        ArgumentNullException.ThrowIfNull(room);

        ArgumentNullException.ThrowIfNull(actorId);

        var reply = await SendCaveRequestAsync(
            "talk",
            new Dictionary<string, object?>
            {
                ["room"] = room,
                ["actor"] = actorId
            });

        return reply.Chat;
    }

    /// <summary>
    ///     Asynchronously casts this character's one vote on the open choice. An option marked unavailable is refused.
    /// </summary>
    /// <param name="choiceId">The open choice's id.</param>
    /// <param name="optionId">The id of the option to vote for.</param>
    /// <returns>The server's reply.</returns>
    /// <exception cref="ArgumentNullException">choiceId</exception>
    /// <exception cref="ArgumentNullException">optionId</exception>
    public Task<GameResponseData> CaveVoteAsync(string choiceId, string optionId)
    {
        ArgumentNullException.ThrowIfNull(choiceId);

        ArgumentNullException.ThrowIfNull(optionId);

        return SendCaveRequestAsync(
            "vote",
            new Dictionary<string, object?>
            {
                ["choice"] = choiceId,
                ["option"] = optionId
            });
    }

    protected Task<bool> OnCaveAsync(CaveData data)
    {
        //every frame but "ended" restates the whole state; a frame without one is not something to overwrite with
        Character.Cave = data.Ended ? null : data.State ?? Character.Cave;

        return TaskCache.FALSE;
    }

    /// <summary>
    ///     Handles a <c>map_chunk</c> frame; the last piece of a floor files the run's floors into the game data and the
    ///     pathfinder. Every character in the party receives the same stream, so filing a floor twice changes nothing.
    /// </summary>
    /// <param name="data">The frame as the server sent it.</param>
    /// <returns>
    ///     false, so the frame stays available to other handlers.
    /// </returns>
    protected Task<bool> OnMapChunkAsync(MapChunkData data)
    {
        var text = MapChunks.Add(data);

        if (text is null)
            return TaskCache.FALSE;

        var bundle = GeneratedMapBundle.Parse(text);
        Pathfinder.RegisterGeneratedRun(bundle);
        Logger.Info($"Filed {bundle.Floors.Count} floor(s) of dungeon run {bundle.Run}.");

        return TaskCache.FALSE;
    }

    private async Task<GameResponseData> SendCaveRequestAsync(
        string action,
        Dictionary<string, object?>? fields = null,
        int timeoutMs = CAVE_REQUEST_TIMEOUT_MS)
    {
        var requestId = RequestId.Create();
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //every cave reply carries the token, so the place check only keeps a stray interaction reply out
                if (!"interaction".EqualsI(data.Place!) || !requestId.EqualsI(data.RequestId!))
                    return TaskCache.FALSE;

                var result = data.Failed
                    ? source.TrySetResult($"Cave {action} failed. ({data.Reason ?? data.ResponseType.ToString()})")
                    : source.TrySetResult(data);

                return Task.FromResult(result);
            });

        var payload = new Dictionary<string, object?>(fields ?? [])
        {
            ["type"] = "cave",
            ["action"] = action,
            ["request_id"] = requestId
        };

        await Socket.EmitAsync(ALSocketEmitType.Interaction, payload);

        return await source.Task.WithTimeout(timeoutMs);
    }
}