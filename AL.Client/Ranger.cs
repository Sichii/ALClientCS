#region
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
using Common.Logging;
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
    public Ranger(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses 5shot on 5 targets.
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
    public async Task<List<ActionData>> FiveShotAsync(
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

        const string SKILL_NAME = "5shot";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);
        var actions = new List<ActionData>();

        var targetIds = new[]
        {
            targetId1,
            targetId2,
            targetId3,
            targetId4,
            targetId5
        };

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (disabled)"),
                    GameResponseType.Cooldown when SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on targets. (on cooldown)"),
                    GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (level too low)"),
                    GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (no mp)"),
                    GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (wrong weapon type)"),

                    //the server collapses the whole volley into the first hit's action object, which carries no "success"
                    GameResponseType.Data when !data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(Expectation.Success),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on targets. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _                               => false
                };

                return Task.FromResult(result);
            });

        //5shot will hit a bunch of targets, and we'll receive them 1 at a time
        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME))
                {
                    actions.Add(data);

                    return TaskCache.TRUE;
                }

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                ids = targetIds
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();

        return actions;
    }

    /// <summary>
    ///     Asynchronously uses 4fingers on a target.
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
    public async Task FourFingers(string targetId)
    {
        const string SKILL_NAME = "4fingers";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when data.TargetId.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when data.TargetId.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                    GameResponseType.NonFriendlyTarget => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (not a friendly target)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                    _                     => false
                };

                return Task.FromResult(result);
            });

        using var evalCallback = Socket.On<EvalData>(
            ALSocketMessageType.Eval,
            data =>
            {
                Match match;

                if (!string.IsNullOrEmpty(data.Code)
                    && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                    && match.Groups[1]
                            .Value
                            .EqualsI(SKILL_NAME))
                    return Task.FromResult(source.TrySetResult(Expectation.Success));

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                id = targetId
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously uses HuntersMark on a target.
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
    ///     Failed to use 'huntersmark' on {targetId}. ({reason})
    /// </exception>
    public async Task<ActionData> HuntersMarkAsync(string targetId)
    {
        const string SKILL_NAME = "huntersmark";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.AttackFailed when data.TargetId.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when data.TargetId.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when data.TargetId.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                    _                     => false
                };

                return Task.FromResult(result);
            });

        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                    return Task.FromResult(source.TrySetResult(data));

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                id = targetId
            });

        return await source.Task.WithNetworkTimeout();
    }

    /// <summary>
    ///     Asynchronously uses PiercingShot on a target.
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
    public async Task<ActionData> PiercingShotAsync(string targetId)
    {
        const string SKILL_NAME = "piercingshot";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    //attack_failed names only the target, never the skill, so it cannot be narrowed any further
                    GameResponseType.AttackFailed when targetId.EqualsI(data.TargetId!) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _                     => false
                };

                return Task.FromResult(result);
            });

        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                    return Task.FromResult(source.TrySetResult(data));

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                id = targetId
            });

        return await source.Task.WithNetworkTimeout();
    }

    /// <summary>
    ///     Asynchronously uses PoisonArrow on a target.
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
    ///     Failed to use 'poisonarrow'. ({reason})
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'poisonarrow' on {targetId}. ({reason})
    /// </exception>
    public async Task<ActionData> PoisonArrowAsync(string targetId)
    {
        const string SKILL_NAME = "poisonarrow";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var poison = Character.Inventory.FindItem("poison");

        if (poison == null)
            throw new InvalidOperationException($"Failed to use '{SKILL_NAME}' on {targetId}. (no poison)");

        var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    //attack_failed names only the target, never the skill, so it cannot be narrowed any further
                    GameResponseType.AttackFailed when targetId.EqualsI(data.TargetId!) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _                     => false
                };

                return Task.FromResult(result);
            });

        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                    return Task.FromResult(source.TrySetResult(data));

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                id = targetId
            });

        return await source.Task.WithNetworkTimeout();
    }

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
    ///     An <see cref="IALAPIClient" /> with your authorization credentials.
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
    public static async Task<Ranger> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IALAPIClient apiClient)
    {
        if (string.IsNullOrEmpty(characterName))
            throw new ArgumentNullException(nameof(characterName));

        ArgumentNullException.ThrowIfNull(apiClient);

        var logger = new FormattedLogger(characterName, LogManager.GetLogger<ALSocketClient>());
        var socketClient = new ALSocketClient(logger);

        var client = new Ranger(characterName, apiClient, socketClient);

        await client.ConnectAsync(region, identifier);

        return client;
    }

    /// <summary>
    ///     Asynchronously uses PoisonArrow on a target.
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
    public async Task<ActionData> SupershotAsync(string targetId)
    {
        const string SKILL_NAME = "supershot";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    //attack_failed names only the target, never the skill, so it cannot be narrowed any further
                    GameResponseType.AttackFailed when targetId.EqualsI(data.TargetId!) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when targetId.EqualsI(data.TargetId!) && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                    GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on {targetId}. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _                     => false
                };

                return Task.FromResult(result);
            });

        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                    return Task.FromResult(source.TrySetResult(data));

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                id = targetId
            });

        return await source.Task.WithNetworkTimeout();
    }

    /// <summary>
    ///     Asynchronously uses 3shot on 3 targets.
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
    public async Task<List<ActionData>> ThreeShotAsync(string targetId1, string targetId2, string targetId3)
    {
        if (string.IsNullOrEmpty(targetId1))
            throw new ArgumentNullException(nameof(targetId1));

        if (string.IsNullOrEmpty(targetId2))
            throw new ArgumentNullException(nameof(targetId2));

        if (string.IsNullOrEmpty(targetId3))
            throw new ArgumentNullException(nameof(targetId3));

        const string SKILL_NAME = "3shot";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);
        var actions = new List<ActionData>();

        var targetIds = new[]
        {
            targetId1,
            targetId2,
            targetId3
        };

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (disabled)"),
                    GameResponseType.Cooldown when SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on targets. (on cooldown)"),
                    GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (level too low)"),
                    GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (no mp)"),
                    GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}' on targets. (wrong weapon type)"),

                    //the server collapses the whole volley into the first hit's action object, which carries no "success"
                    GameResponseType.Data when !data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(Expectation.Success),
                    _ when data.Failed && SKILL_NAME.EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}' on targets. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _                               => false
                };

                return Task.FromResult(result);
            });

        //3shot will hit a bunch of targets, and we'll receive them 1 at a time
        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME))
                {
                    actions.Add(data);

                    return TaskCache.TRUE;
                }

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME,
                ids = targetIds
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();

        return actions;
    }

    /// <summary>
    ///     Uses the 'track' skill and returns the players it locates within range, nearest first
    ///     (node/server.js:9499).
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