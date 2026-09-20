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
///     The Cave of Many Dreams, the daily dungeon. Its floors are generated per run and streamed over map_chunk rather
///     than carried in G; its state rides the cave event onto <see cref="SocketClient.Model.Character.Cave" />; and every
///     request is an interaction of type "cave" with an action, answered on game_response under the request id.
/// </summary>
public abstract partial class ALClient
{
    /// <summary>
    ///     How long to wait for the party to be pulled through the cave gate. The game's own client waits this long, because
    ///     the server settles a whole party's entry or vote before it answers.
    /// </summary>
    private const int CAVE_ENTER_TIMEOUT_MS = 150_000;

    private const int CAVE_REQUEST_TIMEOUT_MS = 10_000;

    private readonly MapChunkAssembler MapChunks = new();

    /// <summary>
    ///     Buys the one item a merchant encounter offers, from the party's purse. The character has to stand near enough (
    ///     <see cref="CaveShop.Nearby" />) and the item can be bought once.
    /// </summary>
    public Task<GameResponseData> CaveBuyAsync(string room)
        => CaveRequestAsync(
            "buy",
            new Dictionary<string, object?>
            {
                ["room"] = room
            });

    /// <summary>
    ///     Asks the keeper to pull the party in. The party has to stand near him on main, out of combat, and the account needs
    ///     its visit for the day. Resolves once the run has started.
    /// </summary>
    public Task<GameResponseData> CaveEnterAsync() => CaveRequestAsync("enter", null, CAVE_ENTER_TIMEOUT_MS);

    /// <summary>
    ///     Leaves the run for good. Works anywhere inside, fallen or mid-vote.
    /// </summary>
    public Task<GameResponseData> CaveExitAsync() => CaveRequestAsync("exit");

    /// <summary>
    ///     The account's standing with the dungeon: whether it can enter now, and when the visit comes back if not.
    /// </summary>
    public async Task<CaveVisit> CaveInfoAsync()
    {
        var reply = await CaveRequestAsync("info");

        return reply.Visit ?? throw new InvalidOperationException("Cave info reply carried no visit.");
    }

    private async Task<GameResponseData> CaveRequestAsync(
        string action,
        Dictionary<string, object?>? fields = null,
        int timeoutMs = CAVE_REQUEST_TIMEOUT_MS)
    {
        var requestId = RequestId.New();
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

    /// <summary>
    ///     Chats with a passing traveler. Pauses nothing and starts no vote.
    /// </summary>
    public async Task<CaveChat?> CaveTalkAsync(string room, string actorId)
    {
        var reply = await CaveRequestAsync(
            "talk",
            new Dictionary<string, object?>
            {
                ["room"] = room,
                ["actor"] = actorId
            });

        return reply.Chat;
    }

    /// <summary>
    ///     Casts this character's one vote on the open choice. An option marked unavailable is refused.
    /// </summary>
    public Task<GameResponseData> CaveVoteAsync(string choiceId, string optionId)
        => CaveRequestAsync(
            "vote",
            new Dictionary<string, object?>
            {
                ["choice"] = choiceId,
                ["option"] = optionId
            });

    protected Task<bool> OnCaveAsync(CaveData data)
    {
        //every frame but "ended" restates the whole state; a frame without one is not something to overwrite with
        Character.Cave = data.Ended ? null : data.State ?? Character.Cave;

        return TaskCache.FALSE;
    }

    /// <summary>
    ///     Handles a <c>map_chunk</c> frame: a floor arrives in pieces, and the last one files the run's floors into the game
    ///     data and the pathfinder. Every character in the party receives the same stream, so filing a floor a second time
    ///     changes nothing.
    /// </summary>
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
}