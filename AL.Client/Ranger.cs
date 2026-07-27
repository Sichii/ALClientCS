#region
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains ranger specific functionality.
/// </summary>
public class Ranger : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Ranger" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the ranger.
    /// </param>
    /// <param name="apiClient">
    ///     An API client implementation.
    /// </param>
    /// <param name="socketClient">
    ///     A socket client implementation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     name
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     socketClient
    /// </exception>
    public Ranger(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses 5Shot on up to five targets.
    /// </summary>
    /// <param name="targetId1">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId2">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId3">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId4">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId5">
    ///     The id of a target.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    ///     <br />
    ///     Information about the projectiles from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId#
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '5shot' on targets. ({reason})
    /// </exception>
    public Task<List<ActionData>> FiveShotAsync(
        string targetId1,
        string targetId2,
        string targetId3,
        string targetId4,
        string targetId5)
    {
        if (string.IsNullOrEmpty(targetId1))
            throw new ArgumentNullException(nameof(targetId1));

        if (string.IsNullOrEmpty(targetId2))
            throw new ArgumentNullException(nameof(targetId2));

        if (string.IsNullOrEmpty(targetId3))
            throw new ArgumentNullException(nameof(targetId3));

        if (string.IsNullOrEmpty(targetId4))
            throw new ArgumentNullException(nameof(targetId4));

        if (string.IsNullOrEmpty(targetId5))
            throw new ArgumentNullException(nameof(targetId5));

        return UseSkillCoreAsync(
            "5shot",
            completion: SkillCompletion.ResponseData,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null,
            collectActions: true,
            payload: new
            {
                name = "5shot",
                ids = new[]
                {
                    targetId1,
                    targetId2,
                    targetId3,
                    targetId4,
                    targetId5
                }
            });
    }

    /// <summary>
    ///     Asynchronously uses 4Fingers on a target, stopping it from moving or attacking.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '4fingers' on {targetId}. ({reason})
    /// </exception>
    public Task FourFingersAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("4fingers", targetId);
    }

    /// <summary>
    ///     Asynchronously uses HuntersMark on a target, raising the damage it takes.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <remarks>
    ///     This produces no projectile — the server answers with a <c>ui</c> frame and the cooldown, nothing else.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'huntersmark' on {targetId}. ({reason})
    /// </exception>
    public Task HuntersMarkAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("huntersmark", targetId);
    }

    /// <summary>
    ///     Asynchronously uses PiercingShot on a target, ignoring some of its armor.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'piercingshot' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> PiercingShotAsync(string targetId)
        => UseProjectileSkillAsync(
            "piercingshot",
            targetId,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null);

    /// <summary>
    ///     Asynchronously uses PoisonArrow on a target, poisoning it.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the poison to use. Left unset, the server picks the last poison in your inventory.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'poisonarrow' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> PoisonArrowAsync(string targetId, int? inventorySlot = null)
        => UseProjectileSkillAsync(
            "poisonarrow",
            targetId,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null,
            inventorySlot: inventorySlot);

    /// <summary>
    ///     Asynchronously creates a Ranger client and connects.
    ///     <br />
    /// </summary>
    /// <param name="characterName">
    ///     The name of the character to log in as.
    /// </param>
    /// <param name="region">
    ///     The region to log into.
    /// </param>
    /// <param name="identifier">
    ///     The identifier suffic for the region.
    /// </param>
    /// <param name="apiClient">
    ///     An <see cref="IAlApiClient" /> with your authorization credentials.
    /// </param>
    /// <returns>
    ///     <see cref="Ranger" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Ranger> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Ranger(name, api, socket));

    /// <summary>
    ///     Asynchronously uses Supershot on a target, hitting it from far outside normal range.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'supershot' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> SupershotAsync(string targetId)
        => UseProjectileSkillAsync(
            "supershot",
            targetId,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null);

    /// <summary>
    ///     Asynchronously uses 3Shot on up to three targets.
    /// </summary>
    /// <param name="targetId1">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId2">
    ///     The id of a target.
    /// </param>
    /// <param name="targetId3">
    ///     The id of a target.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    ///     <br />
    ///     Information about the projectiles from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetId#
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '3shot' on targets. ({reason})
    /// </exception>
    public Task<List<ActionData>> ThreeShotAsync(string targetId1, string targetId2, string targetId3)
    {
        if (string.IsNullOrEmpty(targetId1))
            throw new ArgumentNullException(nameof(targetId1));

        if (string.IsNullOrEmpty(targetId2))
            throw new ArgumentNullException(nameof(targetId2));

        if (string.IsNullOrEmpty(targetId3))
            throw new ArgumentNullException(nameof(targetId3));

        return UseSkillCoreAsync(
            "3shot",
            completion: SkillCompletion.ResponseData,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null,
            collectActions: true,
            payload: new
            {
                name = "3shot",
                ids = new[]
                {
                    targetId1,
                    targetId2,
                    targetId3
                }
            });
    }

    /// <summary>
    ///     Uses the 'track' skill and returns the players it locates within range, nearest first (node/server.js:9499).
    /// </summary>
    /// <returns>
    ///     <see cref="IReadOnlyList{T}" /> of <see cref="TrackData" />
    ///     <br />
    ///     The tracked players, sorted ascending by distance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'track'. ({reason})
    /// </exception>
    public async Task<IReadOnlyList<TrackData>> TrackAsync()
    {
        const string SKILL_NAME = "track";

        var source = new TaskCompletionSource<Expectation<IReadOnlyList<TrackData>>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

        using var trackCallback = Socket.On<TrackData[]>(
            ALSocketMessageType.Track,
            data =>
            {
                source.TrySetResult(data);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME
            });

        return (await source.Task.WithNetworkTimeout()).Result;
    }
}