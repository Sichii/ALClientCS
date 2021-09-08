using System;
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
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Client
{
    /// <summary>
    ///     <inheritdoc cref="ALClient" /> <br />
    ///     Contains priest specific functionality.
    /// </summary>
    public class Priest : ALClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Priest" /> class.
        /// </summary>
        /// <param name="characterName">The name of the priest.</param>
        /// <param name="apiClient">An API client implementation.</param>
        /// <param name="socketClient">A socket client implementation.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        /// <exception cref="ArgumentNullException">socketClient</exception>
        public Priest(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
            : base(characterName, apiClient, socketClient) { }

        /// <summary>
        ///     Asynchronously uses AbsorbSins on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'absorb' on {targetId}. ({reason})</exception>
        public async Task AbsorbSinsAsync(string targetId)
        {
            const string SKILL_NAME = "absorb";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.AttackFailed when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) =>
                            source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                        GameResponseType.Cooldown when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                        GameResponseType.TooFar when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                        GameResponseType.NonFriendlyTarget => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}' on {targetId}. (not a friendly target)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                        _                     => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Curse on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <returns>
        ///     <see cref="ActionData" /> <br />
        ///     Information about the attack's projectile.
        /// </returns>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'curse' on {targetId}. ({reason})</exception>
        public async Task<ActionData> CurseAsync(string targetId)
        {
            const string SKILL_NAME = "curse";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

            var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.AttackFailed when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) =>
                            source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                        GameResponseType.Cooldown when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                        GameResponseType.TooFar when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                        _                     => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var actionCallback = Socket.On<ActionData>(ALSocketMessageType.Action, data =>
                {
                    if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                        return Task.FromResult(source.TrySetResult(data));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            return await source.Task.WithNetworkTimeout().ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously uses DarkBlessing.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'darkblessing'. ({reason})</exception>
        public async Task DarkBlessingAsync()
        {
            const string SKILL_NAME = "darkblessing";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        _                        => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Heal on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <returns>
        ///     <see cref="ActionData" /> <br />
        ///     Information about the heal's projectile.
        /// </returns>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'heal' on {targetId}. ({reason})</exception>
        public async Task<ActionData> HealAsync(string targetId)
        {
            const string SKILL_NAME = "heal";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

            var source = new TaskCompletionSource<Expectation<ActionData>>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.AttackFailed when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}' on {targetId}. (failed)"),
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                        GameResponseType.TooFar when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                        _                     => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var actionCallback = Socket.On<ActionData>(ALSocketMessageType.Action, data =>
                {
                    if (data.AttackerId.EqualsI(Character.Id) && data.Source.EqualsI(SKILL_NAME) && data.Target.EqualsI(targetId))
                        return Task.FromResult(source.TrySetResult(data));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            return await source.Task.WithNetworkTimeout().ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously uses PartyHeal.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'partyheal'. ({reason})</exception>
        public async Task PartyHealAsync()
        {
            const string SKILL_NAME = "partyheal";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        _                        => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses PhaseOut.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'phaseout'. ({reason})</exception>
        public async Task PhaseOutAsync()
        {
            const string SKILL_NAME = "phaseout";

            var shadowStone = Character.Inventory.FindItem("shadowstone");

            if (shadowStone == null)
                throw new InvalidOperationException($"Failed to use '{SKILL_NAME}'. (no shadowstone)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP    => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        _                        => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Revive on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'revive' on {targetId}. ({reason})</exception>
        public async Task ReviveAsync(string targetId)
        {
            const string SKILL_NAME = "revive";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

            var essenceOfLife = Character.Inventory.FindItem("essenceoflife");

            if (essenceOfLife == null)
                throw new InvalidOperationException($"Failed to use '{SKILL_NAME}' on {targetId}. (no essenceoflife)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                        GameResponseType.Cooldown when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                        GameResponseType.TooFar when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                        GameResponseType.NonFriendlyTarget => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}' on {targetId}. (not a friendly target)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                        _                     => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously creates a Priest client and connects. <br />
        /// </summary>
        /// <param name="characterName">The name of the character to log in as.</param>
        /// <param name="region">The region to log into.</param>
        /// <param name="identifier">The identifier suffic for the region.</param>
        /// <param name="apiClient">An <see cref="IALAPIClient" /> with your authorization credentials.</param>
        /// <returns>
        ///     <see cref="Priest" />
        /// </returns>
        /// <exception cref="ArgumentNullException">characterName</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        public static async Task<Priest> StartAsync(string characterName, ServerRegion region, ServerId identifier, IALAPIClient apiClient)
        {
            if (string.IsNullOrEmpty(characterName))
                throw new ArgumentNullException(nameof(characterName));

            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            var logger = new FormattedLogger(characterName, LogManager.GetLogger<ALSocketClient>());
            var socketClient = new ALSocketClient(logger);

            var client = new Priest(characterName, apiClient, socketClient);
            await client.ConnectAsync(region, identifier).ConfigureAwait(false);

            return client;
        }
    }
}