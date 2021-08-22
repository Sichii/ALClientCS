using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AL.APIClient;
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
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Client
{
    /// <summary>
    ///     <inheritdoc cref="ALClient" /> <br />
    ///     Contains merchant specific functionality.
    /// </summary>
    public class Merchant : ALClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Merchant" /> class.
        /// </summary>
        /// <param name="characterName">The name of the merchant.</param>
        /// <param name="apiClient">An API client implementation.</param>
        /// <param name="socketClient">A socket client implementation.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        /// <exception cref="ArgumentNullException">socketClient</exception>
        public Merchant(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
            : base(characterName, apiClient, socketClient) { }

        /// <summary>
        ///     Asynchronously uses Fishing. <br />
        ///     <b>USEABLE BUT INCOMPLETE, I don't own a fishing rod lmaokai</b>
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'fishing'. ({reason})</exception>
        //TODO: complete fishing callbacks
        public async Task FishingAsync()
        {
            const string SKILL_NAME = "fishing";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                        _                               => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            /*
            await using var uiDataCallback = Socket.On<UIData>(ALSocketMessageType.UI, data =>
            {
                var result = data.UIDataType switch
                {
                    UIDataType.FishingFail => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (failed)"),
                    UIDataType.FishingNone => source.TrySetResult(default),
                    UIDataType.FishingStart => source.TrySetResult(default)
                }
            })
            */

            await Socket.Emit(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MassProduction.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'massproduction'. ({reason})</exception>
        public async Task MassProductionAsync()
        {
            const string SKILL_NAME = "massproduction";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillSuccess when data.Name.EqualsI(SKILL_NAME) => source.TrySetResult(Expectation.Success),
                        _ => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await Socket.Emit(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MassProductionPP.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'massproductionpp'. ({reason})</exception>
        public async Task MassProductionPPAsync()
        {
            const string SKILL_NAME = "massproductionpp";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillSuccess when data.Name.EqualsI(SKILL_NAME) => source.TrySetResult(Expectation.Success),
                        _ => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await Socket.Emit(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Mining. <br />
        ///     <b>USEABLE BUT INCOMPLETE, I don't own a pickaxe lmaokai</b>
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'mining'. ({reason})</exception>
        //TODO: complete mining callbacks
        public async Task MiningAsync()
        {
            const string SKILL_NAME = "mining";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                        _                               => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            /*
            await using var uiDataCallback = Socket.On<UIData>(ALSocketMessageType.UI, data =>
            {
                var result = data.UIDataType switch
                {
                    UIDataType.MiningFail => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (failed)"),
                    UIDataType.MiningNone => source.TrySetResult(default),
                    UIDataType.MiningStart => source.TrySetResult(default)
                }
            })
            */

            await Socket.Emit(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MLuck on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'mluck' on {targetId}. ({reason})</exception>
        public async Task MLuckAsync(string targetId)
        {
            const string SKILL_NAME = "mluck";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

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

            await Socket.Emit(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously opens the merchant stand, favoring the computer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to open stand. ({reason})</exception>
        public async Task OpenStandAsync()
        {
            if (Character.Stand != Stand.None)
                return;

            var stand = Character.Inventory.FindItem("computer")
                        ?? Character.Inventory.FindItem(item => item.GetData()?.Type == ItemType.Stand);

            if (stand == null)
                throw new InvalidOperationException("Failed to open stand. (no stand)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    if (data.Stand != Stand.None)
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.Emit(ALSocketEmitType.Merchant, new { num = stand.Index }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously creates a Merchant client and connects. <br />
        /// </summary>
        /// <param name="characterName">The name of the character to log in as.</param>
        /// <param name="region">The region to log into.</param>
        /// <param name="identifier">The identifier suffic for the region.</param>
        /// <param name="apiClient">An <see cref="ALAPIClient" /> with your authorization credentials.</param>
        /// <returns>
        ///     <see cref="Merchant" />
        /// </returns>
        /// <exception cref="ArgumentNullException">characterName</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        public static async Task<Merchant> StartAsync(string characterName, ServerRegion region, ServerId identifier, ALAPIClient apiClient)
        {
            if (string.IsNullOrEmpty(characterName))
                throw new ArgumentNullException(nameof(characterName));

            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            var logger = new FormattedLogger(characterName, LogManager.GetLogger<ALSocketClient>());
            var socketClient = new ALSocketClient(logger);

            var client = new Merchant(characterName, apiClient, socketClient);
            await client.ConnectAsync(region, identifier).ConfigureAwait(false);

            return client;
        }

        //TODO: Throw Stuff... but i dont think anyone will ever use it so it's extremely low priority
    }
}