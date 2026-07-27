#region
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Helpers;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains paladin specific functionality.
/// </summary>
public class Paladin : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Paladin" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the paladin.
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
    public Paladin(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously toggles MShield, trading damage for damage reduction.
    /// </summary>
    /// <remarks>
    ///     This is a toggle and the server sends no cooldown for it, so the call completes on the shield condition
    ///     changing — whichever way it went.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'mshield'. ({reason})
    /// </exception>
    public Task MShieldAsync() => UseSkillCoreAsync("mshield", completion: SkillCompletion.OnCondition(Condition.MShield));

    /// <summary>
    ///     Asynchronously uses Purify on a target.
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
    ///     Failed to use 'purify' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> PurifyAsync(string targetId) => UseProjectileSkillAsync("purify", targetId);

    /// <summary>
    ///     Asynchronously uses SelfHeal, healing yourself.
    /// </summary>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'selfheal'. ({reason})
    /// </exception>
    public async Task<ActionData> SelfHealAsync()
    {
        //the server heals the paladin by attacking them, so this answers with a projectile aimed at yourself
        var actions = await UseSkillCoreAsync("selfheal", completion: SkillCompletion.Action);

        if (actions.Count == 0)
            throw new InvalidOperationException("The server acknowledged 'selfheal' but sent no projectile.");

        return actions[0];
    }

    /// <summary>
    ///     Asynchronously uses Smash on a target.
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
    ///     Failed to use 'smash' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> SmashAsync(string targetId) => UseProjectileSkillAsync("smash", targetId);

    /// <summary>
    ///     Asynchronously creates a Paladin client and connects.
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
    ///     <see cref="Paladin" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Paladin> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Paladin(name, api, socket));
}