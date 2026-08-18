#region
using System.Net;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
///     Contains priest specific functionality.
/// </summary>
public class Priest : ALClient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Priest" /> class.
    /// </summary>
    /// <param name="characterName">
    ///     The name of the priest.
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
    public Priest(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>
    ///     Asynchronously uses AbsorbSins on a target, taking the aggro of everything attacking them.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <remarks>
    ///     Absorbing from a non-friendly target costs six times the mp and fails one time in twenty. The server takes the
    ///     cooldown before reporting that failure, so an unlucky cast reports success.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'absorb' on {targetId}. ({reason})
    /// </exception>
    public Task AbsorbSinsAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("absorb", targetId);
    }

    /// <summary>
    ///     Asynchronously uses Curse on a target, weakening it.
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
    ///     Failed to use 'curse' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> CurseAsync(string targetId) => UseProjectileSkillAsync("curse", targetId);

    /// <summary>
    ///     Asynchronously uses DarkBlessing, raising the damage of your whole party.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'darkblessing'. ({reason})
    /// </exception>
    public Task DarkBlessingAsync() => UseSkillCoreAsync("darkblessing");

    /// <summary>
    ///     Asynchronously uses Heal on a target.
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
    ///     Failed to use 'heal' on {targetId}. ({reason})
    /// </exception>
    public Task<ActionData> HealAsync(string targetId) => UseProjectileSkillAsync("heal", targetId);

    /// <summary>
    ///     Asynchronously uses PartyHeal, healing every member of your party.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'partyheal'. ({reason})
    /// </exception>
    public Task PartyHealAsync() => UseSkillCoreAsync("partyheal");

    /// <summary>
    ///     Asynchronously uses PhaseOut, making you untargetable for a short time.
    /// </summary>
    /// <param name="inventorySlot">
    ///     The slot holding the shadowstone to use. Left unset, the server picks the last one in your inventory.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'phaseout'. ({reason})
    /// </exception>
    public Task PhaseOutAsync(int? inventorySlot = null) => UseSkillCoreAsync("phaseout", inventorySlot: inventorySlot);

    /// <summary>
    ///     Asynchronously uses Revive on a dead target.
    /// </summary>
    /// <param name="targetId">
    ///     The id of the target.
    /// </param>
    /// <param name="inventorySlot">
    ///     The slot holding the essence of life to use. Left unset, the server picks the last one in your inventory.
    /// </param>
    /// <remarks>
    ///     The essence is consumed before the server checks the gravestone, so a target below full hp costs one and
    ///     throws.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     targetId
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to use 'revive' on {targetId}. ({reason})
    /// </exception>
    public Task ReviveAsync(string targetId, int? inventorySlot = null)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync(
            "revive",
            targetId,
            //the gravestone rejection is answered by revive_failed alone - the reject beside it carries no "failed",
            //so without this the skill_timeout that follows would settle the await as a success
            extraFailure: static data => data.ResponseType == GameResponseType.ReviveFailed ? "gravestone not fully healed" : null,
            inventorySlot: inventorySlot);
    }

    /// <summary>
    ///     Asynchronously creates a Priest client and connects.
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
    ///     <see cref="Priest" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     characterName
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     apiClient
    /// </exception>
    public static Task<Priest> StartAsync(
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
            static (name, api, socket) => new Priest(name, api, socket),
            proxy);
}