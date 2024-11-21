#region
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Definitions;
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
///     Contains warrior specific functionality.
/// </summary>
public sealed class Warrior : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Warrior" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the warrior.
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
    public Warrior(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses Agitate.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'agitate'. ({reason})
    /// </exception>
    public async Task AgitateAsync()
    {
        const string SKILL_NAME = "agitate";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    _                        => false
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
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously uses Charge.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'charge'. ({reason})
    /// </exception>
    public async Task ChargeAsync()
    {
        const string SKILL_NAME = "charge";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        var charging = Character.Conditions.ContainsKey(Condition.Charging);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    _                        => false
                };

                return Task.FromResult(result);
            });

        //charge has no eval
        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                //if we werent charging before, and we are now.. we know we just used it
                //this is like this because of how hitchikers are handled
                if (!charging && data.Conditions.ContainsKey(Condition.Charging))
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously uses Cleave.
    /// </summary>
    /// <returns>
    ///     <see cref="List{T}" />
    ///     <br />
    ///     A collection of projectiles resulting from this attack.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'cleave'. ({reason})
    /// </exception>
    public async Task<List<ActionData>> CleaveAsync()
    {
        const string SKILL_NAME = "cleave";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);
        var actions = new List<ActionData>();

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                    _                               => false
                };

                return Task.FromResult(result);
            });

        //a cleave will hit a bunch of targets, and we'll receive them 1 at a time
        using var actionCallback = Socket.On<ActionData>(
            ALSocketMessageType.Action,
            data =>
            {
                if (data.AttackerId.EqualsI(Character.Id) && data.Source!.EqualsI(SKILL_NAME))
                {
                    actions.Add(data);

                    return TaskCache.TRUE;
                }

                return TaskCache.FALSE;
            });

        //eval comes after we receive all the actions
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
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();

        return actions;
    }

    /// <summary>
    ///     Asynchronously uses HardShell.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'hardshell'. ({reason})
    /// </exception>
    public async Task HardShellAsync()
    {
        const string SKILL_NAME = "hardshell";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        var hardShelled = Character.Conditions.ContainsKey(Condition.HardShell);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    _                        => false
                };

                return Task.FromResult(result);
            });

        //hardShell has no eval
        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                //if we werent hardShelled before, and we are now.. we know we just used it
                //this is like this because of how hitchikers are handled
                if (!hardShelled && data.Conditions.ContainsKey(Condition.HardShell))
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Skill,
            new
            {
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously creates a Warrior client and connects.
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
    ///     <see cref="Warrior" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static async Task<Warrior> StartAsync(
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

        var client = new Warrior(characterName, apiClient, socketClient);

        await client.ConnectAsync(region, identifier);

        return client;
    }

    /// <summary>
    ///     Asynchronously uses Stomp.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'stomp'. ({reason})
    /// </exception>
    public async Task StompAsync()
    {
        const string SKILL_NAME = "stomp";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                    _                               => false
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
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously uses Taunt on a target.
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
    ///     Failed to use 'taunt' on {targetId}. ({reason})
    /// </exception>
    public async Task<ActionData> TauntAsync(string targetId)
    {
        const string SKILL_NAME = "taunt";

        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.AttackFailed when data.TargetId!.EqualsI(targetId) && data.Place!.EqualsI(SKILL_NAME) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                    GameResponseType.Cooldown when data.TargetId!.EqualsI(targetId) && data.Place!.EqualsI(SKILL_NAME) =>
                        source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                    GameResponseType.TooFar when data.TargetId!.EqualsI(targetId) && data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
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
                if (data.AttackerId.EqualsI(Character.Id)
                    && (data.Source ?? data.Type)!.EqualsI(SKILL_NAME)
                    && data.Target.EqualsI(targetId))
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
    ///     Asynchronously uses WarCry.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'warcry'. ({reason})
    /// </exception>
    public async Task WarCryAsync()
    {
        const string SKILL_NAME = "warcry";

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                    GameResponseType.Cooldown when data.Place!.EqualsI(SKILL_NAME) => source.TrySetResult(
                        $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                    GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                    GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                    _                        => false
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
                name = SKILL_NAME
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }
}