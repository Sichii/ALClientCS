#region
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Helpers;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains rogue specific functionality.
/// </summary>
public class Rogue : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Rogue" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the rogue.
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
    public Rogue(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses Invis, disappearing into the shadows.
    /// </summary>
    /// <remarks>
    ///     The server takes this skill's cooldown when the invisibility ends rather than when it starts, so the call
    ///     completes on going invisible instead. Casting it while already invisible does nothing and will time out.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'invis'. ({reason})
    /// </exception>
    public Task InvisAsync() => UseSkillCoreAsync("invis", completion: SkillCompletion.OnCondition(Condition.Invis));

    /// <summary>
    ///     Asynchronously uses MentalBurst on a target.
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
    ///     Failed to use 'mentalburst' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> MentalBurstAsync(string targetId) => UseProjectileSkillAsync("mentalburst", targetId);

    /// <summary>
    ///     Asynchronously uses PCoat, coating your weapon so your hits poison what they land on.
    /// </summary>
    /// <param name="inventorySlot">
    ///     The slot holding the poison to use. Left unset, the server picks the last poison in your inventory.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'pcoat'. ({reason})
    /// </exception>
    public Task PCoatAsync(int? inventorySlot = null) => UseSkillCoreAsync("pcoat", inventorySlot: inventorySlot);

    /// <summary>
    ///     Asynchronously starts Pickpocket on a target, trying to steal a pvp-marked item from them.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <remarks>
    ///     This returns once the server accepts the cast, which starts the attempt rather than finishing it. The steal
    ///     resolves later and mostly fails; the long cooldown is taken only when it succeeds.
    ///     <br />
    ///     Pickpocket is <c>persistent</c>, and the server restores its cooldown on a frame that does not ride login — so
    ///     between connecting and your first state-changing action, <see cref="ALClient.Cooldowns" /> has no entry for it
    ///     and it reads as ready when it is not. Casting anyway costs one call and fails with "(on cooldown)".
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'pickpocket' on {targetId}. ({reason})
    /// </exception>
    public Task PickpocketAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("pickpocket", targetId, SkillCompletion.ResponseData);
    }

    /// <summary>
    ///     Asynchronously uses QuickPunch on a target, an extra hit between normal attacks.
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
    ///     Failed to use 'quickpunch' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> QuickPunchAsync(string targetId) => UseProjectileSkillAsync("quickpunch", targetId);

    /// <summary>
    ///     Asynchronously uses QuickStab on a target, an extra hit between normal attacks.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <returns>
    ///     <see cref="ActionData" />
    ///     <br />
    ///     Information about the projectile from this skill.
    /// </returns>
    /// <remarks>
    ///     This shares its cooldown with QuickPunch, so the server acknowledges it under that name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'quickstab' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> QuickStabAsync(string targetId) => UseProjectileSkillAsync("quickstab", targetId);

    /// <summary>
    ///     Asynchronously uses RSpeed on a target, raising their movement speed.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'rspeed' on {targetId}. ({reason})
    /// </exception>
    public Task RSpeedAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("rspeed", targetId);
    }

    /// <summary>
    ///     Asynchronously uses ShadowStrike, spending a shadowstone to strike every monster in range.
    /// </summary>
    /// <param name="inventorySlot">
    ///     The slot holding the shadowstone to use. Left unset, the server picks the last one in your inventory.
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" /> of <see cref="ActionData" />
    ///     <br />
    ///     A collection of projectiles resulting from this attack.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'shadowstrike'. ({reason})
    /// </exception>
    public Task<List<ActionData>> ShadowStrikeAsync(int? inventorySlot = null)
        => UseSkillCoreAsync("shadowstrike", collectActions: true, inventorySlot: inventorySlot);

    /// <summary>
    ///     Asynchronously creates a Rogue client and connects.
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
    ///     <see cref="Rogue" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Rogue> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Rogue(name, api, socket));
}