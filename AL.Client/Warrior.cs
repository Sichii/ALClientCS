#region
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Helpers;
using AL.Core.Helpers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
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
    public Warrior(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses Agitate, pulling every nearby monster onto yourself.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'agitate'. ({reason})
    /// </exception>
    public Task AgitateAsync() => UseSkillCoreAsync("agitate");

    /// <summary>
    ///     Asynchronously uses Charge, raising your movement speed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'charge'. ({reason})
    /// </exception>
    public Task ChargeAsync() => UseSkillCoreAsync("charge");

    /// <summary>
    ///     Asynchronously uses Cleave, striking every monster around you.
    /// </summary>
    /// <returns>
    ///     <see cref="List{T}" /> of <see cref="ActionData" />
    ///     <br />
    ///     A collection of projectiles resulting from this attack.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'cleave'. ({reason})
    /// </exception>
    public Task<List<ActionData>> CleaveAsync()
        => UseSkillCoreAsync(
            "cleave",
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null,
            collectActions: true);

    /// <summary>
    ///     Asynchronously uses Dash, jumping to a nearby point over any obstacle in the way.
    /// </summary>
    /// <param name="x">
    ///     The x coordinate to jump to. Must be within 50 units, or the server rejects the jump.
    /// </param>
    /// <param name="y">
    ///     The y coordinate to jump to. Must be within 50 units, or the server rejects the jump.
    /// </param>
    /// <remarks>
    ///     Dash has no cooldown, so the server acknowledges it with nothing and this returns as soon as the emit is
    ///     written. A rejected jump is reported only by the character's position not changing.
    /// </remarks>
    public Task DashAsync(float x, float y)
        => UseSkillCoreAsync(
            "dash",
            completion: SkillCompletion.Immediate,
            payload: new
            {
                name = "dash",
                x,
                y
            });

    /// <summary>
    ///     Asynchronously uses HardShell, trading movement speed for armor.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'hardshell'. ({reason})
    /// </exception>
    public Task HardShellAsync() => UseSkillCoreAsync("hardshell");

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
    ///     An <see cref="IAlApiClient" /> with your authorization credentials.
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
        IAlApiClient apiClient)
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
    ///     Asynchronously uses Stomp, stunning every monster around you.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'stomp'. ({reason})
    /// </exception>
    public Task StompAsync()
        => UseSkillCoreAsync(
            "stomp",
            extraFailure: static data => data.ResponseType == GameResponseType.SkillCantWType ? "wrong weapon type" : null);

    /// <summary>
    ///     Asynchronously uses Taunt on a target, pulling it onto yourself.
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
    public Task<ActionData> TauntAsync(string targetId) => UseProjectileSkillAsync("taunt", targetId);

    /// <summary>
    ///     Asynchronously uses WarCry, buffing your whole party.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'warcry'. ({reason})
    /// </exception>
    public Task WarCryAsync() => UseSkillCoreAsync("warcry");
}
