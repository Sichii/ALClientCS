#region
using System.Net;
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
    ///     Asynchronously uses 5Shot on one to five targets.
    /// </summary>
    /// <param name="targetIds">
    ///     The ids of the targets. Five is a ceiling and not a requirement: the server takes however many it is given, so a
    ///     shorter volley fires fewer arrows for the same mana.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    ///     <br />
    ///     Information about the projectiles from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetIds
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     targetIds is empty, longer than five, or holds a null or empty id.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '5shot' on targets. ({reason})
    /// </exception>
    public Task<List<ActionData>> FiveShotAsync(params string[] targetIds) => MultiShotAsync("5shot", 5, targetIds);

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
    ///     This produces no projectile — the server answers with a
    ///     <c>
    ///         ui
    ///     </c>
    ///     frame and the cooldown, nothing else.
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
    ///     The shared body of the two multishots, which differ only in name and ceiling.
    /// </summary>
    /// <remarks>
    ///     The ceiling is checked here rather than left to the server, which silently truncates a longer list
    ///     (node/server.js:9575) and would leave the caller believing arrows it paid for had landed.
    /// </remarks>
    private Task<List<ActionData>> MultiShotAsync(string skillName, int maxTargets, string[] targetIds)
    {
        ArgumentNullException.ThrowIfNull(targetIds);

        if ((targetIds.Length == 0) || (targetIds.Length > maxTargets))
            throw new ArgumentException(
                $"'{skillName}' takes 1 to {maxTargets} targets, but was given {targetIds.Length}.",
                nameof(targetIds));

        if (targetIds.Any(string.IsNullOrEmpty))
            throw new ArgumentException($"'{skillName}' was given a null or empty target id.", nameof(targetIds));

        return UseSkillCoreAsync(
            skillName,
            targetIds: targetIds,
            completion: SkillCompletion.ResponseData,
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null,
            collectActions: true,
            payload: new
            {
                name = skillName,
                ids = targetIds
            });
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
    /// <param name="proxy">
    ///     The proxy to reach the game through, or null for the machine's own connection.
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
        IAlApiClient apiClient,
        IWebProxy? proxy = null)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Ranger(name, api, socket),
            proxy);

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
    ///     Asynchronously uses 3Shot on one to three targets.
    /// </summary>
    /// <param name="targetIds">
    ///     The ids of the targets. Three is a ceiling and not a requirement: the server takes however many it is given, so a
    ///     shorter volley fires fewer arrows for the same mana.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    ///     <br />
    ///     Information about the projectiles from this skill.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     targetIds
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     targetIds is empty, longer than three, or holds a null or empty id.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use '3shot' on targets. ({reason})
    /// </exception>
    public Task<List<ActionData>> ThreeShotAsync(params string[] targetIds) => MultiShotAsync("3shot", 3, targetIds);

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